using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using MCNBTEditor.ColourMap.WPF.Controls;
using MCNBTEditor.Core;
using MCNBTEditor.Core.AdvancedContextService;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.ColourMap.Maps.Brushes {
    public class ColourBrushViewModel : BrushViewModel {
        private ColourRGBA colour;
        public ColourRGBA Colour {
            get => this.colour;
            set {
                this.RaisePropertyChanged(ref this.colour, value);
                if (!this.HasBeenModified)
                    this.HasBeenModified = true;
            }
        }

        public byte A { get => this.Colour.A; }
        public byte R { get => this.Colour.R; }
        public byte G { get => this.Colour.G; }
        public byte B { get => this.Colour.B; }

        public ICommand ShowPickerCommand { get; }

        public ColourBrushViewModel(BrushItemViewModel owningItem) : base(owningItem) {
            this.ShowPickerCommand = new RelayCommand(this.ShowPickerAction);
        }

        public void ShowPickerAction() {
            ColourPickerWindow window = new ColourPickerWindow();
            ColourRGBA oldColour = this.colour;
            window.Colour = Color.FromArgb(oldColour.A, oldColour.R, oldColour.G, oldColour.B);
            window.ColourChanged += c => {
                this.Colour = new ColourRGBA(c.R, c.G, c.B, c.A);
            };

            if (window.ShowDialog() == true) {
                Color c = window.Colour;
                this.Colour = new ColourRGBA(c.R, c.G, c.B, c.A);
                this.HasBeenModified = true;
                this.OwningItem.HasBeenModified = true;
            }
            else {
                this.Colour = oldColour;
                this.HasBeenModified = false;
                this.OwningItem.HasBeenModified = false;
            }
        }

        public void GetContext(List<IContextEntry> list) {
            list.Add(new CommandContextEntry("Edit Colour", this.ShowPickerCommand));
        }
    }
}