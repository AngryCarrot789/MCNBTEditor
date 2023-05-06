using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Converters;
using System.Windows.Threading;
using MCNBTEditor.Core.Actions.Contexts;
using MCNBTEditor.Core.AdvancedContextService;
using MCNBTEditor.Core.Utils;
using MCNBTEditor.Shortcuts;

namespace MCNBTEditor.AdvancedContextService {
    public class AdvancedContextMenu : ContextMenu {
        public static readonly DependencyProperty ContextGeneratorProperty = DependencyProperty.RegisterAttached(
            "ContextGenerator", typeof(IWPFContextGenerator), typeof(AdvancedContextMenu), new PropertyMetadata(null, OnContextGeneratorPropertyChanged));

        public static readonly DependencyProperty ContextProviderProperty =
            DependencyProperty.RegisterAttached(
                "ContextProvider",
                typeof(IContextProvider),
                typeof(AdvancedContextMenu),
                new PropertyMetadata(null, OnContextProviderPropertyChanged));

        public static readonly DependencyProperty ContextEntrySourceProperty =
            DependencyProperty.RegisterAttached(
                "ContextEntrySource",
                typeof(IEnumerable<IContextEntry>),
                typeof(AdvancedContextMenu),
                new PropertyMetadata(null, OnContextProviderPropertyChanged));

        public static readonly DependencyProperty IsMenuEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsMenuEnabled",
                typeof(bool),
                typeof(AdvancedContextMenu),
                new FrameworkPropertyMetadata(BoolBox.True, FrameworkPropertyMetadataOptions.Inherits));

        private static readonly ContextMenuEventHandler MenuOpenHandler = OnContextMenuOpening;
        private static readonly ContextMenuEventHandler MenuCloseHandler = OnContextMenuClosing;
        private static readonly ContextMenuEventHandler MenuOpenHandlerForGenerable = OnContextMenuOpeningForGenerable;
        private static readonly ContextMenuEventHandler MenuCloseHandlerForGenerable = OnContextMenuClosingForGenerable;

        private object currentItem;

        public AdvancedContextMenu() {

        }

        public static DependencyObject CreateChildMenuItem(object item) {
            FrameworkElement element;
            if (item is ActionContextEntry) {
                element = new AdvancedActionMenuItem();
            }
            else if (item is ShortcutCommandContextEntry) {
                element = new AdvancedShortcutMenuItem();
            }
            else if (item is BaseContextEntry) {
                element = new AdvancedMenuItem();
            }
            else if (item is SeparatorEntry) {
                element = new Separator();
            }
            else {
                throw new Exception("Unknown item type: " + item?.GetType());
            }

            // element.IsVisibleChanged += ElementOnIsVisibleChanged;
            return element;
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            if (item is MenuItem || item is Separator)
                return true;
            this.currentItem = item;
            return false;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            object item = this.currentItem;
            this.currentItem = null;
            if (this.UsesItemContainerTemplate) {
                DataTemplate dataTemplate = this.ItemContainerTemplateSelector.SelectTemplate(item, this);
                if (dataTemplate != null) {
                    object obj = dataTemplate.LoadContent();
                    if (obj is MenuItem || obj is Separator) {
                        return (DependencyObject) obj;
                    }

                    throw new InvalidOperationException("Invalid data template object: " + obj);
                }
            }

            return CreateChildMenuItem(item);
        }

        private static void OnContextProviderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (ReferenceEquals(e.OldValue, e.NewValue)) {
                return;
            }

            ContextMenuService.RemoveContextMenuOpeningHandler(d, MenuOpenHandler);
            ContextMenuService.RemoveContextMenuClosingHandler(d, MenuCloseHandler);
            if (e.NewValue != null) {
                GetOrCreateContextMenu(d);
                ContextMenuService.AddContextMenuOpeningHandler(d, MenuOpenHandler);
                ContextMenuService.AddContextMenuClosingHandler(d, MenuCloseHandler);
            }
        }

        private static List<IContextEntry> GetContexEntries(DependencyObject target) {
            List<IContextEntry> list = new List<IContextEntry>();
            if (GetContextProvider(target) is IContextProvider provider) {
                provider.GetContext(list);
            }
            else if (GetContextEntrySource(target) is IEnumerable<IContextEntry> entries) {
                list.AddRange(entries);
            }

            return list;
        }

        private static void OnContextGeneratorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue == e.NewValue) {
                return;
            }

            ContextMenuService.RemoveContextMenuOpeningHandler(d, MenuOpenHandlerForGenerable);
            ContextMenuService.RemoveContextMenuClosingHandler(d, MenuCloseHandlerForGenerable);
            if (e.NewValue != null) {
                GetOrCreateContextMenu(d);
                ContextMenuService.AddContextMenuOpeningHandler(d, MenuOpenHandlerForGenerable);
                ContextMenuService.AddContextMenuClosingHandler(d, MenuCloseHandlerForGenerable);
            }
        }

        public static void OnContextMenuOpeningForGenerable(object sender, ContextMenuEventArgs e) {
            if (sender is DependencyObject sourceObject && e.OriginalSource is DependencyObject targetObject) {
                IWPFContextGenerator generator = GetContextGenerator(sourceObject);
                if (generator != null) {
                    List<IContextEntry> list = new List<IContextEntry>();
                    generator.Generate(list, sourceObject, targetObject, VisualTreeUtils.GetDataContext(targetObject));
                    if (list.Count < 1) {
                        return;
                    }

                    AdvancedContextMenu menu = GetOrCreateContextMenu(sourceObject);
                    menu.Items.Clear();
                    foreach (IContextEntry entry in CleanEntries(list)) {
                        menu.Items.Add(entry);
                    }
                }
            }
        }

        public static void OnContextMenuClosingForGenerable(object sender, ContextMenuEventArgs e) {
            if (sender is DependencyObject targetElement && ContextMenuService.GetContextMenu(targetElement) is ContextMenu menu) {
                menu.Dispatcher.Invoke(() => {
                    menu.Items.Clear();
                }, DispatcherPriority.DataBind);
            }
        }

        public static void OnContextMenuOpening(object sender, ContextMenuEventArgs e) {
            if (sender is DependencyObject targetElement) {
                // A workaround for the problem which is this entire idea of view models generating context menus;
                // they shouldn't be generated by view models but it's the only memory efficient way I have implemented.
                // There's no way that 1000s of tree items are having their own unique context menu instances
                // with all their menu items. It doesn't get more inefficient than that
                if (!GetIsMenuEnabled(targetElement)) {
                    e.Handled = true;
                    return;
                }

                List<IContextEntry> context = GetContexEntries(targetElement);
                if (context == null || context.Count < 1) {
                    return;
                }

                AdvancedContextMenu menu = GetOrCreateContextMenu(targetElement);
                menu.Items.Clear();
                foreach (IContextEntry entry in CleanEntries(context)) {
                    menu.Items.Add(entry);
                }
            }
        }

        public static IEnumerable<IContextEntry> CleanEntries(List<IContextEntry> entries) {
            IContextEntry lastEntry = null;
            for (int i = 0, end = entries.Count - 1; i <= end; i++) {
                IContextEntry entry = entries[i];
                if (!(entry is SeparatorEntry) || (i != 0 && i != end && !(lastEntry is SeparatorEntry))) {
                    yield return entry;
                }

                lastEntry = entry;
            }
        }

        public static void OnContextMenuClosing(object sender, ContextMenuEventArgs e) {
            if (sender is DependencyObject targetElement && ContextMenuService.GetContextMenu(targetElement) is ContextMenu menu) {
                menu.Dispatcher.Invoke(() => {
                    menu.Items.Clear();
                }, DispatcherPriority.DataBind);
            }
        }

        private static AdvancedContextMenu GetOrCreateContextMenu(DependencyObject targetElement) {
            ContextMenu menu = ContextMenuService.GetContextMenu(targetElement);
            if (!(menu is AdvancedContextMenu advancedMenu)) {
                ContextMenuService.SetContextMenu(targetElement, advancedMenu = new AdvancedContextMenu());
            }

            return advancedMenu;
        }

        public static void SetContextGenerator(DependencyObject element, IWPFContextGenerator value) {
            element.SetValue(ContextGeneratorProperty, value);
        }

        public static IWPFContextGenerator GetContextGenerator(DependencyObject element) {
            return (IWPFContextGenerator) element.GetValue(ContextGeneratorProperty);
        }

        public static void SetContextProvider(DependencyObject element, IContextProvider value) {
            element.SetValue(ContextProviderProperty, value);
        }

        public static IContextProvider GetContextProvider(DependencyObject element) {
            return (IContextProvider) element.GetValue(ContextProviderProperty);
        }

        public static void SetContextEntrySource(DependencyObject element, IEnumerable<IContextEntry> value) {
            element.SetValue(ContextEntrySourceProperty, value);
        }

        public static IEnumerable<IContextEntry> GetContextEntrySource(DependencyObject element) {
            return (IEnumerable<IContextEntry>) element.GetValue(ContextEntrySourceProperty);
        }

        public static void SetIsMenuEnabled(DependencyObject element, bool value) {
            element.SetValue(IsMenuEnabledProperty, value);
        }

        public static bool GetIsMenuEnabled(DependencyObject element) {
            return (bool) element.GetValue(IsMenuEnabledProperty);
        }
    }
}