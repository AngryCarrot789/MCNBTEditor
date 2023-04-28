using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MCNBTEditor.Core.AdvancedContextService;

namespace MCNBTEditor.AdvancedContextService {
    public class AdvancedContextMenu : ContextMenu {
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

        private static readonly ContextMenuEventHandler MenuOpenHandler = OnContextMenuOpening;
        private static readonly ContextMenuEventHandler MenuCloseHandler = OnContextMenuClosing;

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

        public static List<IContextEntry> GetContexEntries(DependencyObject target) {
            List<IContextEntry> list = new List<IContextEntry>();
            if (GetContextProvider(target) is IContextProvider provider) {
                provider.GetContext(list);
            }
            else if (GetContextEntrySource(target) is IEnumerable<IContextEntry> entries) {
                list.AddRange(entries);
            }

            return list;
        }

        public static void OnContextMenuOpening(object sender, ContextMenuEventArgs e) {
            if (sender is DependencyObject targetElement) {
                List<IContextEntry> context = GetContexEntries(targetElement);
                if (context == null || context.Count < 1) {
                    return;
                }

                AdvancedContextMenu menu = GetOrCreateContextMenu(targetElement);
                menu.Items.Clear();
                IContextEntry lastEntry = null;
                foreach (IContextEntry entry in context) {
                    if (lastEntry is SeparatorEntry && entry is SeparatorEntry) {
                        continue;
                    }

                    menu.Items.Add(entry);
                    lastEntry = entry;
                }
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
    }
}