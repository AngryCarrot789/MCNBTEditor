using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using MCNBTEditor.Core.Explorer;

namespace MCNBTEditor.Controls {
    public class ExtendedTreeView : TreeView, IExtendedTree {
        public static readonly DependencyProperty UseItemCommandProperty = DependencyProperty.Register("UseItemCommand", typeof(ICommand), typeof(ExtendedTreeView), new PropertyMetadata(null));

        public ICommand UseItemCommand {
            get => (ICommand) this.GetValue(UseItemCommandProperty);
            set => this.SetValue(UseItemCommandProperty, value);
        }

        public event SelectionChangedEventHandler<BaseTreeItemViewModel> SelectionChanged;
        private ScrollViewer PART_ScrollViewier;

        public ExtendedTreeView() {

        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e) {
            if (this.PART_ScrollViewier != null && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                if (e.Delta < 0) {
                    // scroll right
                    this.PART_ScrollViewier.LineRight();
                    this.PART_ScrollViewier.LineRight();
                    this.PART_ScrollViewier.LineRight();
                }
                else if (e.Delta > 0) {
                    this.PART_ScrollViewier.LineLeft();
                    this.PART_ScrollViewier.LineLeft();
                    this.PART_ScrollViewier.LineLeft();
                }
                else {
                    return;
                }

                e.Handled = true;
                return;
            }

            base.OnPreviewMouseWheel(e);
        }

        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e) {
            base.OnSelectedItemChanged(e);
            if (e.Handled) {
                return;
            }

            this.SelectionChanged?.Invoke(e.OldValue as BaseTreeItemViewModel, e.NewValue as BaseTreeItemViewModel);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_ScrollViewier = (ScrollViewer) this.GetTemplateChild("_tv_scrollviewer_");
            // this.RequestBringIntoView += this.OnRequestBringIntoView;
        }

        public static TreeViewItem ContainerFromItemRecursive(ItemContainerGenerator root, object item) {
            if (root == null)
                return null;

            if (root.ContainerFromItem(item) is TreeViewItem treeViewItem)
                return treeViewItem;
            foreach (object subItem in root.Items) {
                treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem;
                TreeViewItem search = ContainerFromItemRecursive(treeViewItem?.ItemContainerGenerator, item);
                if (search != null)
                    return search;
            }

            return null;
        }

        public static bool AccumulateItemContainerGeneratorChainForItem(ItemContainerGenerator root, object item, IList<ItemContainerGenerator> generators) {
            if (root.ContainerFromItem(item) is TreeViewItem treeViewItem) {
                generators.Add(root);
                return true;
            }

            foreach (object subItem in root.Items) {
                if (ReferenceEquals(subItem, item) || (subItem != null && subItem.Equals(item))) {
                    generators.Add(root);
                    return true;
                }

                if ((treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem) == null) {
                    continue;
                }

                if (AccumulateItemContainerGeneratorChainForItem(treeViewItem.ItemContainerGenerator, item, generators)) {
                    return true;
                }
            }

            return false;
        }

        public static TreeViewItem GetAndGenerateContainerUntilItemFound(UIElement rootElement, ItemContainerGenerator root, object item) {
            if (root == null) {
                return null;
            }

            if (root.ContainerFromItem(item) is TreeViewItem treeViewItem)
                return treeViewItem;

            foreach (object subItem in root.Items) {
                if ((treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem) == null) {
                    rootElement.UpdateLayout();
                    GenerateChildren(root);
                    rootElement.UpdateLayout();
                    if ((treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem) == null) {
                        continue;
                    }
                }

                if (ReferenceEquals(subItem, item) || (subItem != null && subItem.Equals(item))) {
                    return treeViewItem;
                }

                TreeViewItem search = GetAndGenerateContainerUntilItemFound(treeViewItem, treeViewItem.ItemContainerGenerator, item);
                if (search != null)
                    return search;
            }

            return null;

            // if (root.ContainerFromItem(item) is TreeViewItem treeViewItem)
            //     return treeViewItem;
            // foreach (object subItem in root.Items) {
            //     if ((treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem) != null) {
            //         TreeViewItem search = ContainerFromItemRecursive(treeViewItem.ItemContainerGenerator, item);
            //         if (search != null)
            //             return search;
            //     }
            // }
            // return null;
        }

        public static void GenerateChildren(IItemContainerGenerator generator) {
            using (generator.StartAt(new GeneratorPosition(-1, 0), GeneratorDirection.Forward)) {
                while (generator.GenerateNext() is UIElement next) {
                    generator.PrepareItemContainer(next);
                }
            }
        }

        public void SetSelectedFile(object item) {
            if (item is TreeViewItem obj) {
                obj.IsSelected = true;
                obj.BringIntoView();
                // ChangeSelectionMethod.Invoke(this, new object[] {
                //     this.ItemContainerGenerator.ItemFromContainer(obj),
                //     obj,
                //     true
                // });
            }
            else {
                TreeViewItem treeViewItem = ContainerFromItemRecursive(this.ItemContainerGenerator, item);
                if (treeViewItem != null) {
                    // for (ItemsControl parent = ItemsControlFromItemContainer(treeViewItem); parent != null; parent = ItemsControlFromItemContainer(parent)) {
                    //     if (parent is TreeViewItem treeItem) {
                    //         treeItem.IsExpanded = true;
                    //         // treeItem.ExpandSubtree();
                    //     }
                    //     else {
                    //         break;
                    //     }
                    // }

                    treeViewItem.IsSelected = true;
                    treeViewItem.BringIntoView();
                }

                // ChangeSelectionMethod.Invoke(this, new object[] {
                //     item,
                //     this.ItemContainerGenerator.ContainerFromItem(item),
                //     true
                // });
            }
            // if (file is BaseNBTCollectionViewModel folder) {
            //     for (BaseNBTCollectionViewModel parent = folder.Parent; parent != null; parent = parent.Parent) {
            //         if (!parent.IsExpanded && parent.CanExpand) {
            //             parent.IsExpanded = true;
            //         }
            //     }
            //     folder.IsExpanded = true;
            // }
        }

        BaseTreeItemViewModel IExtendedTree.GetSelectedItem() {
            return this.SelectedItem as BaseTreeItemViewModel;
        }

        public bool IsItemExpanded(BaseTreeItemViewModel item) {
            if (item == null) {
                return false;
            }

            DependencyObject container = ContainerFromItemRecursive(this.ItemContainerGenerator, item);
            return container is TreeViewItem treeItem && treeItem.IsExpanded;
        }

        public void SetExpanded(BaseTreeItemViewModel nbt) {
            if (nbt == null) {
                return;
            }

            DependencyObject container = ContainerFromItemRecursive(this.ItemContainerGenerator, nbt);
            if (container is TreeViewItem treeItem) {
                if (!treeItem.IsExpanded) {
                    treeItem.IsExpanded = true;
                }
            }

            // BaseViewModel.SetInternalData(nbt, nameof(TreeViewItem.IsExpanded), true);
        }

        public bool IsExpanded(BaseTreeItemViewModel nbt) {
            return this.IsItemExpanded(nbt);
            // return BaseViewModel.GetInternalData<bool>(nbt, nameof(TreeViewItem.IsExpanded));
        }

        public async Task<bool> RepeatExpandHierarchyFromRootAsync(IEnumerable<BaseTreeItemViewModel> items) {
            // TODO: optimise this!!!!

            List<BaseTreeItemViewModel> list = items as List<BaseTreeItemViewModel> ?? items.ToList();
            bool result = false;
            for (int i = 0; i < list.Count; i++) {
                result |= await this.Dispatcher.InvokeAsync(() => this.ExpandHierarchyFromRoot(list, true), DispatcherPriority.Background);
            }

            return result;
        }

        public Task ExpandItemSubTree(BaseTreeItemViewModel item) {
            DependencyObject container = ContainerFromItemRecursive(this.ItemContainerGenerator, item);
            if (container is TreeViewItem treeItem) {
                this.ExpandSubtree(treeItem);
            }

            return Task.CompletedTask;
        }

        public async Task<bool> MainExpandHierarchy(IEnumerable<int> items) {
            return false;
            // List<int> list = items as List<int> ?? items.ToList();
            // return await await this.Dispatcher.InvokeAsync(() => this.MainExpandHierarchyInternal(list), DispatcherPriority.Background);
            // bool result = false;
            // for (int i = 0; i < list.Count; i++) {
            //     result |= await await this.Dispatcher.InvokeAsync(() => this.MainExpandHierarchyInternal(list), DispatcherPriority.Background);
            // }
            // return result;
        }

        // public async Task<bool> MainExpandHierarchyInternal(IEnumerable<int> items) {
        //     ItemsControl control = this;
        //     ItemContainerGenerator root = control.ItemContainerGenerator;
        //     TreeViewItem lastItem = null;
        //     using (IEnumerator<int> enumerator = items.GetEnumerator()) {
        //         while (enumerator.MoveNext()) {
        //             int itemIndex = enumerator.Current;
        //             if (root.Status == GeneratorStatus.NotStarted) {
        //                 control.UpdateLayout();
        //             }
//
        //             if (root.Status == GeneratorStatus.NotStarted || root.Status == GeneratorStatus.Error) {
        //                 return false;
        //             }
        //             // GenerateChildren(root);
        //             TreeViewItem treeItem;
        //             if ((treeItem = root.ContainerFromIndex(itemIndex) as TreeViewItem) == null) {
        //                 using (root.GenerateBatches()) {
        //                     IItemContainerGenerator gen = root;
        //                     using (gen.StartAt(gen.GeneratorPositionFromIndex(itemIndex), GeneratorDirection.Forward)) {
        //                         if (gen.GenerateNext() is TreeViewItem treeItem2) {
        //                             gen.PrepareItemContainer(treeItem2);
        //                             this.AddVisualChild(treeItem2);
        //                         }
        //                     }
        //                 }
//
        //                 if ((treeItem = root.ContainerFromIndex(itemIndex) as TreeViewItem) == null) {
        //                     return false;
        //                 }
        //             }
//
        //             if (!treeItem.IsExpanded) {
        //                 treeItem.IsExpanded = true;
        //             }
//
        //             this.UpdateLayout();
        //             lastItem?.UpdateLayout();
        //             root = treeItem.ItemContainerGenerator;
        //             if (lastItem != null) {
        //                 lastItem.IsExpanded = true;
        //                 lastItem.BringIntoView();
        //             }
//
        //             lastItem = treeItem;
        //             treeItem.IsSelected = true;
        //             await this.Dispatcher.InvokeAsync(() => {
        //                 lastItem.BringIntoView();
        //             }, DispatcherPriority.Background);
        //         }
        //     }
//
        //     if (lastItem != null) {
        //         lastItem.IsSelected = true;
        //         await this.Dispatcher.InvokeAsync(() => {
        //             lastItem.BringIntoView();
        //         }, DispatcherPriority.Background);
        //     }
//
        //     return true;
        // }

        public bool ExpandHierarchyFromRoot(IEnumerable<BaseTreeItemViewModel> items, bool select) {
            ItemsControl control = this;
            ItemContainerGenerator root = control.ItemContainerGenerator;
            TreeViewItem lastItem = null;
            using (IEnumerator<BaseTreeItemViewModel> enumerator = items.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    if (root.Status == GeneratorStatus.NotStarted) {
                        control.UpdateLayout();
                    }

                    if (root.Status == GeneratorStatus.NotStarted || root.Status == GeneratorStatus.Error) {
                        return false;
                    }

                    // GenerateChildren(root);
                    BaseTreeItemViewModel item = enumerator.Current;
                    TreeViewItem treeItem;
                    if ((treeItem = root.ContainerFromItem(item) as TreeViewItem) == null) {
                        using (root.GenerateBatches()) {
                            IItemContainerGenerator gen = root;
                            using (gen.StartAt(new GeneratorPosition(-1, 0), GeneratorDirection.Forward)) {
                                DependencyObject generated;
                                while ((generated = gen.GenerateNext()) != null) {
                                    gen.PrepareItemContainer(generated);
                                }
                            }
                        }

                        if ((treeItem = root.ContainerFromItem(item) as TreeViewItem) == null) {
                            return false;
                        }
                    }

                    if (!treeItem.IsExpanded) {
                        treeItem.IsExpanded = true;
                    }

                    root = treeItem.ItemContainerGenerator;
                    lastItem = treeItem;
                    lastItem.BringIntoView();
                }
            }

            if (select && lastItem != null) {
                lastItem.IsSelected = true;
                lastItem.BringIntoView();
            }

            return true;
        }

        public async Task<bool> ExpandAsync(IEnumerable<BaseTreeItemViewModel> items, bool select) {
            ItemContainerGenerator root = this.ItemContainerGenerator;
            TreeViewItem lastItem = null;
            using (IEnumerator<BaseTreeItemViewModel> enumerator = items.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    await this.Dispatcher.InvokeAsync(() => {
                        GenerateChildren(root);
                    });
                    BaseTreeItemViewModel item = enumerator.Current;
                    if (root.ContainerFromItem(item) is TreeViewItem treeItem) {
                        if (!treeItem.IsExpanded) {
                            treeItem.IsExpanded = true;
                        }

                        root = treeItem.ItemContainerGenerator;
                        lastItem = treeItem;
                        await this.Dispatcher.InvokeAsync(() => {
                            lastItem.BringIntoView();
                        });
                    }
                    else {
                        return false;
                    }
                }
            }

            if (select && lastItem != null) {
                lastItem.IsSelected = true;
                await this.Dispatcher.InvokeAsync(() => {
                    lastItem.BringIntoView();
                });
            }

            return true;
        }
    }
}