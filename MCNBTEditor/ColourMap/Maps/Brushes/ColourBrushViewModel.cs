namespace MCNBTEditor.ColourMap.Maps.Brushes {
    public class ColourBrushItemViewModel : BrushItemViewModel {
        private float alpha;
        public float Alpha {
            get => this.alpha;
            set {
                this.RaisePropertyChanged(ref this.alpha, value);
                if (!this.HasBeenModified)
                    this.HasBeenModified = true;
            }
        }

        private float red;
        public float Red {
            get => this.red;
            set {
                this.RaisePropertyChanged(ref this.red, value);
                if (!this.HasBeenModified)
                    this.HasBeenModified = true;
            }
        }

        private float green;
        public float Green {
            get => this.green;
            set {
                this.RaisePropertyChanged(ref this.green, value);
                if (!this.HasBeenModified)
                    this.HasBeenModified = true;
            }
        }

        private float blue;
        public float Blue {
            get => this.blue;
            set {
                this.RaisePropertyChanged(ref this.blue, value);
                if (!this.HasBeenModified)
                    this.HasBeenModified = true;
            }
        }

        public float A { get => this.Alpha; set => this.Alpha = value; }
        public float R { get => this.Red;   set => this.Red = value; }
        public float G { get => this.Green; set => this.Green = value; }
        public float B { get => this.Blue;  set => this.Blue = value; }

        public ColourBrushItemViewModel(ColourSchemaViewModel schema, ColourMapViewModel parent, string id, string name) : base(schema, parent, id, name) {

        }
    }
}