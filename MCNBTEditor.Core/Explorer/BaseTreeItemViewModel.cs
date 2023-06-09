using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.Core.Explorer {
    public abstract class BaseTreeItemViewModel : BaseViewModel {
        public const string UnknownPathElementName = "<unknown>";

        private readonly ObservableCollectionEx<BaseTreeItemViewModel> children;
        private BaseTreeItemViewModel parentItem;
        private bool isValid;
        private bool isReadOnly;

        /// <summary>
        /// A reference to the internal children collection. Only modify in exceptional circumstances
        /// where the functions provided in <see cref="BaseTreeItemViewModel"/> are not enough! You will have to use reflection
        /// to set the <see cref="IsValid"/> property, and potentially more in the future
        /// </summary>
        protected ObservableCollectionEx<BaseTreeItemViewModel> InternalChildren => this.children;

        public ReadOnlyObservableCollection<BaseTreeItemViewModel> Children { get; }

        public BaseTreeItemViewModel ParentItem {
            get => this.parentItem;
            private set => this.RaisePropertyChanged(ref this.parentItem, value);
        }

        public bool IsValid {
            get => this.isValid;
            private set => this.RaisePropertyChanged(ref this.isValid, value);
        }


        /// <summary>
        /// This item is read only and cannot be modified, nor any of the children. Modifying children when
        /// the item is read only may result in undesired results
        /// </summary>
        public bool IsReadOnly {
            get => this.isReadOnly;
            set => this.RaisePropertyChanged(ref this.isReadOnly, value);
        }

        /// <summary>
        /// Whether this item is empty, as in, has no children. This will not throw even if <see cref="CanHoldChildren"/> is false
        /// </summary>
        public bool IsEmpty => this.children.Count < 1;

        /// <summary>
        /// The number of children in this item
        /// </summary>
        public int ChildrenCount => this.children.Count;

        /// <summary>
        /// Whether or not this item has a parent or not (aka "IsNotRoot")
        /// </summary>
        public bool HasParent => this.ParentItem != null;

        /// <summary>
        /// Whether or not this item is the root item (as in, it has no parent)
        /// </summary>
        public bool IsRoot => this.ParentItem == null;

        /// <summary>
        /// Whether this item is allowed to hold children or not
        /// </summary>
        public abstract bool CanHoldChildren { get; }

        public string TreeFullPath => string.Join("/", this.GetPathChain());

        protected BaseTreeItemViewModel() {
            this.children = new ObservableCollectionEx<BaseTreeItemViewModel>();
            this.Children = new ReadOnlyObservableCollection<BaseTreeItemViewModel>(this.children);
            this.children.CollectionChanged += this.OnChildrenCollectionChanged;
        }

        public List<BaseTreeItemViewModel> GetParentChain(bool includeRoot = true) {
            List<BaseTreeItemViewModel> list = new List<BaseTreeItemViewModel>();
            BaseTreeItemViewModel item = this;
            do {
                list.Add(item);
            } while ((item = item.ParentItem) != null && (includeRoot || !item.IsRoot));
            list.Reverse();
            return list;
        }

        public List<int> GetParentChainIndices() {
            List<int> list = new List<int>();
            BaseTreeItemViewModel item = this;
            BaseTreeItemViewModel parent = item.parentItem;
            while (parent != null) {
                list.Add(parent.IndexOf(item));
                parent = (item = parent).parentItem;
            }

            list.Reverse();
            return list;
        }

        public IEnumerable<(BaseTreeItemViewModel, string)> GetParentAndNameChain() {
            foreach (BaseTreeItemViewModel item in this.GetParentChain()) {
                if (item is IHaveTreePath path && path.TreePathPartName is string partName) {
                    yield return (item, partName);
                }
                else {
                    yield return (item, UnknownPathElementName);
                }
            }
        }

        public IEnumerable<string> GetPathChain() {
            return this.GetParentAndNameChain().Select(x => x.Item2);
        }

        protected virtual void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            this.RaisePropertyChanged(nameof(this.ChildrenCount));
            this.RaisePropertyChanged(nameof(this.IsEmpty));
        }

        public bool FindParent<T>(out T value) where T : BaseTreeItemViewModel {
            for (BaseTreeItemViewModel parent = this.ParentItem; parent != null; parent = parent.ParentItem) {
                if (parent is T t) {
                    value = t;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public T FindParent<T>() where T : BaseTreeItemViewModel {
            return this.FindParent(out T value) ? value : null;
        }

        public TResult GetFirstChild<TResult>() {
            return this.children.FirstOfType<TResult>();
        }

        public bool GetFirstChild<TResult>(out TResult result) {
            return this.children.FirstOfType(out result);
        }

        public BaseTreeItemViewModel FindChild(Predicate<BaseTreeItemViewModel> match, BaseTreeItemViewModel def = null) {
            foreach (BaseTreeItemViewModel itemViewModel in this.children) {
                if (match(itemViewModel)) {
                    return itemViewModel;
                }
            }

            return def;
        }

        /// <summary>
        /// Gets this item's absolute root item. When trying to exclude the absolute root: if this
        /// item is already the absolute root, then the current instance is returned. If this item's parent
        /// is absolute root, then the current instance is returned again. Otherwise, calculate the 2nd to last root item
        /// </summary>
        /// <param name="tryExcludeAbsoluteRoot">
        /// Whether to try and exclude the absolute root item or not (that is, the item that has no parent)
        /// </param>
        /// <returns></returns>
        public BaseTreeItemViewModel GetRootParent(bool tryExcludeAbsoluteRoot = true) {
            return GetRootParent(this, tryExcludeAbsoluteRoot);
        }

        public static BaseTreeItemViewModel GetRootParent(BaseTreeItemViewModel root, bool tryExcludeAbsoluteRoot = true) {
            if (root == null)
                return null;

            if (tryExcludeAbsoluteRoot) {
                BaseTreeItemViewModel last = root;
                for (BaseTreeItemViewModel parent = root.ParentItem; parent != null; parent = parent.ParentItem) {
                    last = root;
                    root = parent;
                }

                return last;
            }
            else {
                for (BaseTreeItemViewModel next = root.ParentItem; next != null; next = next.ParentItem) {
                    root = next;
                }

                return root;
            }
        }

        protected void ValidateCanHoldChildren() {
            if (!this.CanHoldChildren) {
                throw new InvalidOperationException("This item cannot contain children");
            }
        }

        protected virtual void AddRange(IEnumerable<BaseTreeItemViewModel> enumerable) {
            this.ValidateCanHoldChildren();
            List<BaseTreeItemViewModel> list = enumerable.ToList();
            this.children.AddRange(list);
            this.EnsureChildren(list, true);
        }

        protected virtual void Add(BaseTreeItemViewModel item) {
            this.ValidateCanHoldChildren();
            this.children.Add(item);
            this.EnsureChild(item, true);
        }

        protected virtual void Insert(int index, BaseTreeItemViewModel item) {
            this.ValidateCanHoldChildren();
            this.children.Insert(index, item);
            this.EnsureChild(item, true);
        }

        protected virtual void InsertRange(int index, IEnumerable<BaseTreeItemViewModel> enumerable) {
            this.ValidateCanHoldChildren();
            List<BaseTreeItemViewModel> list = enumerable.ToList();
            this.children.InsertRange(index, list);
            this.EnsureChildren(list, true);
        }

        protected virtual bool Contains(BaseTreeItemViewModel item) {
            this.ValidateCanHoldChildren();
            return this.children.Contains(item);
        }

        protected virtual bool Remove(BaseTreeItemViewModel item) {
            this.ValidateCanHoldChildren();
            int index = this.IndexOf(item);
            if (index < 0) {
                return false;
            }

            this.RemoveAt(index);
            return true;
        }

        protected virtual void RemoveAll(IEnumerable<BaseTreeItemViewModel> enumerable) {
            this.ValidateCanHoldChildren();
            foreach (BaseTreeItemViewModel item in enumerable) {
                this.Remove(item);
            }
        }

        protected virtual void RemoveAll(Predicate<BaseTreeItemViewModel> canRemove) {
            // this.RemoveAll(this.items.Where(canRemove).ToList());
            this.ValidateCanHoldChildren();
            for (int i = this.children.Count - 1; i >= 0; i--) {
                BaseTreeItemViewModel item = this.children[i];
                if (canRemove(item)) {
                    this.EnsureChild(item, false);
                    this.children.RemoveAt(i);
                }
            }
        }

        protected virtual int IndexOf(BaseTreeItemViewModel item) {
            this.ValidateCanHoldChildren();
            return this.children.IndexOf(item);
        }

        protected virtual void RemoveAt(int index) {
            this.ValidateCanHoldChildren();
            this.EnsureChild(this.children[index], false);
            this.children.RemoveAt(index);
        }

        protected virtual void Clear() {
            this.ValidateCanHoldChildren();
            this.EnsureChildren(this.children, false);
            this.children.Clear();
        }

        protected virtual void ClearAndAdd(BaseTreeItemViewModel item) {
            this.ValidateCanHoldChildren();
            this.EnsureChildren(this.children, false);
            this.children.ClearAndAdd(item);
        }

        protected virtual void ClearAndAddRange(IEnumerable<BaseTreeItemViewModel> items) {
            this.ValidateCanHoldChildren();
            this.EnsureChildren(this.children, false);
            this.children.ClearAndAddRange(items);
        }

        protected virtual void EnsureChild(BaseTreeItemViewModel child, bool valid) {
            if (child != null) {
                child.SetParent(valid ? this : null);
                child.IsValid = valid;
            }
        }

        protected virtual void EnsureChildren(IEnumerable<BaseTreeItemViewModel> childList, bool valid) {
            foreach (BaseTreeItemViewModel item in childList) {
                this.EnsureChild(item, valid);
            }
        }

        protected virtual void SetParent(BaseTreeItemViewModel parent) {
            this.ParentItem = parent;
        }

        public static async Task<List<BaseTreeItemViewModel>> ResolvePathAction(BaseTreeItemViewModel root, string path, bool canUserSelectInvalidReferences = true) {
            int index, lastIndex = 0;
            BaseTreeItemViewModel item = root;
            List<BaseTreeItemViewModel> list = new List<BaseTreeItemViewModel>();
            while ((index = path.IndexOf('/', lastIndex)) >= 0) {
                if (index != 0) { // skip root
                    item = await item.GetChildAction(path, lastIndex, index, canUserSelectInvalidReferences);
                    if (item == null) {
                        return null;
                    }
                    else if (item.CanHoldChildren) {
                        list.Add(item);
                    }
                    else {
                        await IoC.MessageDialogs.ShowMessageAsync("Invalid path", $"Item at path '{path.Substring(0, index)}' cannot hold children (It is of type {(item is BaseTagViewModel tag ? tag.ToString() : item.ToString())})");
                    }
                }

                lastIndex = index + 1;
            }

            item = await item.GetChildAction(path, lastIndex, path.Length, canUserSelectInvalidReferences);
            if (item == null) {
                return null;
            }

            list.Add(item);
            return list;
        }

        public async Task<BaseTreeItemViewModel> GetChildAction(string path, int j, int i, bool canShowUi) {
            if (this.children.Count < 1) {
                if (canShowUi) {
                    await IoC.MessageDialogs.ShowMessageAsync("No children available", $"The item at '{path.Substring(0, j)}' does not contain any children");
                }

                return null;
            }

            string name = path.JSubstring(j, i);
            if (string.IsNullOrEmpty(name)) {
                return !canShowUi ? null : await IoC.ItemSelectorService.SelectItemAsync(this.children, "Invalid element in path", $"The path '{path.Substring(0, i)}' contains an element with an empty name. Select a suitable item to use instead:");
            }

            List<BaseTreeItemViewModel> items = new List<BaseTreeItemViewModel>();
            foreach (BaseTreeItemViewModel child in this.children) {
                if (child is IHaveTreePath pathable && pathable.TreePathPartName == name) {
                    items.Add(child);
                }
            }

            if (items.Count < 1) {
                // finally try to parse the index. the above checks just in case a tag was actually named [3] for example
                if (name[0] == '[' && name[name.Length - 1] == ']') {
                    if (int.TryParse(name.Substring(1, name.Length - 2), out int index) && index >= 0 && index < this.children.Count) {
                        return this.children[index];
                    }
                    else {
                        return !canShowUi ? null : await IoC.ItemSelectorService.SelectItemAsync(this.children, "Invalid array element in path", $"{path.Substring(0, i)} is an invalid array. Select a suitable item to use instead:");
                    }
                }
                else if (name == UnknownPathElementName) {
                    return !canShowUi ? null : await IoC.ItemSelectorService.SelectItemAsync(this.children, "Unknown element in path", $"Path contains an unknown element in {path.Substring(0, j)}. Select a suitable item to use instead:");
                }
                else {
                    return !canShowUi ? null : await IoC.ItemSelectorService.SelectItemAsync(this.children, "No such item in path", $"Unknown child at path: {path.Substring(0, i)}. Select a suitable item to use instead:");
                }
            }
            else if (items.Count == 1) {
                return items[0];
            }
            else {
                return !canShowUi ? null : await IoC.ItemSelectorService.SelectItemAsync(items, "Ambiguous path elements", $"There are {items.Count} children with the name '{name}'. This list is ordered from oldest to newest added; select which one to use:");
            }
        }

        public static string GetNameErrorMessage(string name, string path) {
            if (name[0] == '[' && name[name.Length - 1] == ']') {
                return $"No such child at index '{name.JSubstring(1, name.Length - 1)}' in: '{path}'";
            }
            else {
                return $"No such child by the name of '{name}' in: '{path}'";
            }
        }
    }
}