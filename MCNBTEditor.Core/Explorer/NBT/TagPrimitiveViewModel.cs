using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.Core.Explorer.NBT {
    /// <summary>
    /// A view model for tags end, byte, short, int, long, float, double and string
    /// </summary>
    public class TagPrimitiveViewModel : BaseTagViewModel {
        public override bool CanHoldChildren => false;

        private string data;
        public string Data {
            get => this.data;
            set => this.RaisePropertyChanged(ref this.data, value);
        }

        public ICommand CopyValueCommand { get; }
        public ICommand EditValueCommand { get; }
        public ICommand EditGeneralCommand { get; }

        private TagPrimitiveViewModel(string name, NBTType type, string data) : base(name, type) {
            this.CopyValueCommand = new AsyncRelayCommand(this.CopyValueAction);
            this.EditValueCommand = new AsyncRelayCommand(this.EditValueAction);
            this.EditGeneralCommand = new AsyncRelayCommand(this.EditPrimitiveTagAction);
            this.data = data;
        }

        public TagPrimitiveViewModel(string name, NBTType type) : this(name, type, null) {
            if (type != NBTType.String && (type < NBTType.End || type > NBTType.Double)) {
                throw new Exception($"Invalid NBT type: {type}");
            }
        }

        public async Task CopyValueAction() {
            if (this.Data != null) {
                await ClipboardUtils.SetClipboardOrShowErrorDialog(this.Data);
            }
        }

        public async Task EditValueAction() {
            await IoC.TagEditorService.EditPrimitiveValueAsync($"Edit NBTTag{this.TagType}", "Edit this tag's value", this.TagType, this.Data);
        }

        public async Task EditPrimitiveTagAction() {
            await IoC.TagEditorService.EditPrimitiveTagAsync($"Edit NBTTag{this.TagType}", "Edit this tag", this);
        }

        public override NBTBase ToNBT() {
            switch (this.TagType) {
                case NBTType.End:    return new NBTTagEnd();
                case NBTType.Byte:   return new NBTTagByte(byte.Parse(this.data));
                case NBTType.Short:  return new NBTTagShort(short.Parse(this.data));
                case NBTType.Int:    return new NBTTagInt(int.Parse(this.data));
                case NBTType.Long:   return new NBTTagLong(long.Parse(this.data));
                case NBTType.Float:  return new NBTTagFloat(float.Parse(this.data));
                case NBTType.Double: return new NBTTagDouble(double.Parse(this.data));
                case NBTType.String: return new NBTTagString(this.data);
                default: throw new Exception($"This primitive tag has an invalid type: {this.TagType}. Current class = {this.GetType()}");
            }
        }

        public override BaseTagViewModel Clone() {
            return new TagPrimitiveViewModel(this.Name, this.TagType, this.data);
        }
    }
}