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

        /// <summary>
        /// A reference to the internal children collection. Only modify in exceptional circumstances
        /// where the functions provided in <see cref="BaseTreeItemViewModel"/> are not enough! You will have to use reflection
        /// to set the <see cref="IsValid"/> property, and potentially more in the future
        /// </summary>
        protected ObservableCollectionEx<BaseTreeItemViewModel> InternalChildren => this.children;

        public ReadOnlyObservableCollection<BaseTreeItemViewModel> Children { get; }

        private BaseTreeItemViewModel parentItem;
        public BaseTreeItemViewModel ParentItem {
            get => this.parentItem;
            private set => this.RaisePropertyChanged(ref this.parentItem, value);
        }

        private bool isValid;
        public bool IsValid {
            get => this.isValid;
            private set => this.RaisePropertyChanged(ref this.isValid, value);
        }

        /// <summary>
        /// Calculates the root parent of this item. If the current instance is already root, then the current instance is returned
        /// </summary>
        public BaseTreeItemViewModel RootParent {
            get {
                BaseTreeItemViewModel root = this;
                for (BaseTreeItemViewModel next = this.ParentItem; next != null; next = next.ParentItem)
                    root = next;
                return root;
            }
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
            this.children.CollectionChanged += this.OnChildListModified;
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

        protected virtual void OnChildListModified(object sender, NotifyCollectionChangedEventArgs e) {
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
            ObservableCollectionEx<BaseTreeItemViewModel> list = this.children;
            for (int i = list.Count - 1; i >= 0; i--) {
                BaseTreeItemViewModel item = list[i];
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

        public async Task<List<BaseTreeItemViewModel>> ResolvePathAction(string path) {
            try {
                return await this.ResolvePath(path);
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid path", e.Message ?? "Failed to resolve path: " + path);
                return null;
            }
        }

        public async Task<List<BaseTreeItemViewModel>> ResolvePath(string path) {
            int i, j = 0;
            string name;
            BaseTreeItemViewModel item = this;
            List<BaseTreeItemViewModel> list = new List<BaseTreeItemViewModel>();
            while ((i = path.IndexOf('/', j)) >= 0) {
                if (i != 0) { // skip root
                    name = path.JSubstring(j, i);
                    item = await item.GetChildByName(name);
                    if (item == null) {
                        throw new Exception(GetNameErrorMessage(name, j == 0 ? "<root>" : path.Substring(0, j - 1)));
                    }
                    else if (item is BaseTagCollectionViewModel) {
                        list.Add(item);
                    }
                    else {
                        throw new Exception($"Expected collection at '{(j == 0 ? "<root>" : path.Substring(0, i))}', but got {(item is BaseTagViewModel tag ? tag.NBTType.ToString() : item.ToString())}");
                    }
                }

                j = i + 1;
            }

            item = await item.GetChildByName(name = path.Substring(j));
            if (item == null) {
                throw new Exception(GetNameErrorMessage(name, j == 0 ? "<root>" : path.Substring(0, j - 1)));
            }

            list.Add(item);
            return list;
        }

        public async Task<BaseTreeItemViewModel> GetChildByName(string name) {
            if (string.IsNullOrEmpty(name)) {
                return null;
            }

            List<BaseTreeItemViewModel> ambiguous = new List<BaseTreeItemViewModel>();
            foreach (BaseTreeItemViewModel child in this.InternalChildren) {
                if (child is IHaveTreePath pathable && pathable.TreePathPartName == name) {
                    ambiguous.Add(child);
                }
            }

            if (ambiguous.Count < 1) {
                // finally try to parse the index. the above checks just in case a tag was actually named [3] for example
                if (name[0] == '[' && name[name.Length - 1] == ']') {
                    if (int.TryParse(name.Substring(1, name.Length - 2), out int index) && index >= 0 && index < this.InternalChildren.Count) {
                        return this.InternalChildren[index];
                    }
                }
                else if (name == UnknownPathElementName) {
                    // TODO: Show GUI to let the user select the right child
                    await IoC.MessageDialogs.ShowMessageAsync("Invalid path", "Path contains an unknown element");
                }
            }
            else if (ambiguous.Count == 1) {
                return ambiguous[0];
            }
            else {
                // TODO: Show GUI to let the user select one
                await IoC.MessageDialogs.ShowMessageAsync("Ambiguous path", $"There are {ambiguous.Count} children with the name '{name}'");
            }

            return null;
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