using System;
using MCNBTEditor.Core.NBT;
using MCNBTEditor.Core.Views.Dialogs.UserInputs;
using MCNBTEditor.Core.Views.Dialogs;

namespace MCNBTEditor.Core.Explorer.Dialog {
    public class TagPrimitiveEditorViewModel : BaseConfirmableDialogViewModel {
        private string title;
        public string Title {
            get => this.title;
            set => this.RaisePropertyChanged(ref this.title, value);
        }

        private string message;
        public string Message {
            get => this.message;
            set => this.RaisePropertyChanged(ref this.message, value);
        }

        private string name;
        public string Name {
            get => this.name;
            set => this.RaisePropertyChanged(ref this.name, value);
        }

        private string value;
        public string Value {
            get => this.value;
            set {
                if (this.value == value) {
                    return;
                }

                this.RaisePropertyChanged(ref this.value, value);
                if (!this.IsBoolButtonVisible && this.TagType == NBTType.Byte) {
                    this.IsBoolButtonVisible = true;
                }

                if (this.IsBoolButtonVisible) {
                    switch (value) {
                        case "0": this.IsBooleanChecked = false; break;
                        case "1": this.IsBooleanChecked = true; break;
                        default: this.IsBooleanChecked = null; break;
                    }
                }
            }
        }

        private bool canEditName;
        public bool CanEditName {
            get => this.canEditName;
            set => this.RaisePropertyChanged(ref this.canEditName, value);
        }

        private bool canEditValue;
        public bool CanEditValue {
            get => this.canEditValue;
            set => this.RaisePropertyChanged(ref this.canEditValue, value);
        }

        private bool? isBooleanChecked;
        public bool? IsBooleanChecked {
            get => this.isBooleanChecked;
            set {
                if (this.isBooleanChecked != value) {
                    switch (value) {
                        case true: this.Value = "1"; break;
                        case false: this.Value = "0"; break;
                        case null: this.Value = "2"; break;
                    }

                    this.RaisePropertyChanged(ref this.isBooleanChecked, value);
                }
            }
        }

        private bool isBoolButtonVisible;
        public bool IsBoolButtonVisible {
            get => this.isBoolButtonVisible;
            set => this.RaisePropertyChanged(ref this.isBoolButtonVisible, value);
        }

        private NBTType tagType;
        public NBTType TagType {
            get => this.tagType;
            set {
                switch (value) {
                    case NBTType.ByteArray:
                    case NBTType.IntArray:
                    case NBTType.LongArray:
                    case NBTType.List:
                    case NBTType.Compound:
                        throw new ArgumentOutOfRangeException(nameof(value), value, "Type is not primitive: " + value);
                }

                this.IsBoolButtonVisible = this.tagType == NBTType.Byte;
                this.RaisePropertyChanged(ref this.tagType, value);
            }
        }

        public InputValidator NameValidator { get; set; }

        public InputValidator ValueValidator { get; }

        public TagPrimitiveEditorViewModel() {
            this.ValueValidator = InputValidator.FromFunc((input) => {
                if (!this.CanEditValue) {
                    return null;
                }

                switch (this.TagType) {
                    case NBTType.End: return null;
                    case NBTType.Byte: return byte.TryParse(input, out _) ? null : "Invalid byte value";
                    case NBTType.Short: return short.TryParse(input, out _) ? null : "Invalid short value";
                    case NBTType.Int: return int.TryParse(input, out _) ? null : "Invalid int value";
                    case NBTType.Long: return long.TryParse(input, out _) ? null : "Invalid long value";
                    case NBTType.Float: return float.TryParse(input, out _) ? null : "Invalid float value";
                    case NBTType.Double: return double.TryParse(input, out _) ? null : "Invalid double value";
                    case NBTType.String: {
                        if (!string.IsNullOrEmpty(input) && input.Length > ushort.MaxValue) {
                            return $"String is too long: {input.Length} > {ushort.MaxValue}";
                        }

                        return null;
                    }
                    default: return $"Error: Input tag type ({this.TagType})";
                }
            });
        }

        public NBTBase ToNBT() {
            switch (this.TagType) {
                case NBTType.End: return new NBTTagEnd();
                case NBTType.Byte: { return new NBTTagByte(byte.TryParse(this.Value, out byte x) ? x : default); }
                case NBTType.Short: { return new NBTTagShort(short.TryParse(this.Value, out short x) ? x : default); }
                case NBTType.Int: { return new NBTTagInt(int.TryParse(this.Value, out int x) ? x : default); }
                case NBTType.Long: { return new NBTTagLong(long.TryParse(this.Value, out long x) ? x : default); }
                case NBTType.Float: { return new NBTTagFloat(float.TryParse(this.Value, out float x) ? x : default); }
                case NBTType.Double: { return new NBTTagDouble(double.TryParse(this.Value, out double x) ? x : default); }
                case NBTType.String: { return new NBTTagString(this.Value); }
                default: return null;
            }
        }
    }
}
