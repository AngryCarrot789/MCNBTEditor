using System;
using System.Collections.Generic;
using System.Windows.Input;
using MCNBTEditor.ColourMap.Maps.Brushes;
using MCNBTEditor.Core;
using MCNBTEditor.Core.AdvancedContextService;

namespace MCNBTEditor.ColourMap.Maps {
    public class BrushItemViewModel : BaseMapItemViewModel, IContextProvider {
        private bool hasBeenModified;
        public bool HasBeenModified {
            get => this.hasBeenModified;
            set => this.RaisePropertyChanged(ref this.hasBeenModified, value);
        }

        private BrushViewModel brush;
        public BrushViewModel Brush {
            get => this.brush;
            set {
                this.RaisePropertyChanged(ref this.brush, value);
                if (!this.HasBeenModified)
                    this.HasBeenModified = true;
            }
        }

        public string Id { get; }

        public ICommand ShowPickerCommand { get; }

        public BrushItemViewModel(ColourSchemaViewModel schema, ColourMapViewModel parent, string id, string displayName) : base(schema, parent, displayName) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null, empty or whitespaces", nameof(id));
            this.Id = id;
            this.ShowPickerCommand = new RelayCommand(this.ShowPickerAction);
        }

        private void ShowPickerAction() {
            this.Brush = new ColourBrushViewModel(this);
            ((ColourBrushViewModel) this.brush).ShowPickerAction();
        }

        public void GetContext(List<IContextEntry> list) {
            list.Add(new CommandContextEntry("Edit Colour", this.ShowPickerCommand));
        }
    }
}