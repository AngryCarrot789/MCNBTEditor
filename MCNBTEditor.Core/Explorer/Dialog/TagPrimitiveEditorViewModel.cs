using System;
using System.Collections.Generic;
using System.Text;
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
            set => this.RaisePropertyChanged(ref this.value, value);
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

                this.RaisePropertyChanged(ref this.tagType, value);
            }
        }

        public InputValidator NameValidator { get; set; }

        public InputValidator ValueValidator { get; }

        public TagPrimitiveEditorViewModel() {
            this.ValueValidator = InputValidator.FromFunc((x) => {
                if (!this.CanEditValue) {
                    return null;
                }

                switch (this.TagType) {
                    case NBTType.End: return null;
                    case NBTType.Byte: return byte.TryParse(this.Value, out _) ? null : "Invalid byte value";
                    case NBTType.Short: return short.TryParse(this.Value, out _) ? null : "Invalid short value";
                    case NBTType.Int: return int.TryParse(this.Value, out _) ? null : "Invalid int value";
                    case NBTType.Long: return long.TryParse(this.Value, out _) ? null : "Invalid long value";
                    case NBTType.Float: return float.TryParse(this.Value, out _) ? null : "Invalid float value";
                    case NBTType.Double: return double.TryParse(this.Value, out _) ? null : "Invalid double value";
                    case NBTType.String: return null;
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
