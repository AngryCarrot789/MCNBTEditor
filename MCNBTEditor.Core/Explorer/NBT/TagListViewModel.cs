using System.Collections.Specialized;
using System.Threading.Tasks;
using MCNBTEditor.Core.NBT;

namespace MCNBTEditor.Core.Explorer.NBT {
    public class TagListViewModel : BaseTagCollectionViewModel {
        private NBTType targetType;
        public NBTType TargetType {
            get => this.targetType;
            set => this.RaisePropertyChangedIfChanged(ref this.targetType, value);
        }

        public TagListViewModel(string name = null) : base(name, NBTType.List) {

        }

        public override async Task PasteBinaryTagDataAction(string name, NBTBase nbt) {
            if (this.TargetType != NBTType.End && nbt.TagType != this.TargetType && this.Children.Count > 0) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid type", "This tag list expects items of type " + this.TargetType + ", not " + nbt.TagType + ". Remove all exists items from the list and then paste it in, to switch the type");
                return;
            }

            this.AddChild(CreateFrom(name, nbt));
        }

        public override NBTBase ToNBT() {
            NBTTagList list = new NBTTagList();
            foreach (BaseTagViewModel item in this.ChildTags) {
                list.tags.Add(item.ToNBT());
            }

            return list;
        }

        public override BaseTagViewModel Clone() {
            TagListViewModel list = new TagListViewModel(this.Name);
            foreach (BaseTagViewModel item in this.ChildTags)
                list.Add(item.Clone());
            return list;
        }

        protected override void OnChildListModified(object sender, NotifyCollectionChangedEventArgs e) {
            base.OnChildListModified(sender, e);
            this.TargetType = this.FirstTag?.NBTType ?? NBTType.End;
        }
    }
}