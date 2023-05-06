using System;
using MCNBTEditor.Core;

namespace MCNBTEditor.ColourMap.Maps {
    public class BaseMapItemViewModel : BaseViewModel {
        public ColourSchemaViewModel Schema { get; }

        public ColourMapViewModel Parent { get; }

        public string DisplayName { get; }

        public string FullName { get; }

        private bool isReadOnly;
        public virtual bool IsReadOnly {
            get => this.Parent?.IsReadOnly ?? this.isReadOnly;
            set => this.RaisePropertyChanged(ref this.isReadOnly, value || this.IsReadOnly);
        }

        public BaseMapItemViewModel(ColourSchemaViewModel schema, ColourMapViewModel parent, string displayName, bool isReadOnly = false) {
            if (schema == null)
                throw new ArgumentException("Schema cannot be null, empty or whitespaces", nameof(schema));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Name cannot be null, empty or whitespaces", nameof(displayName));

            this.Schema = schema;
            this.DisplayName = displayName;
            this.Parent = parent;
            this.FullName = parent != null ? (parent.FullName + "/" + displayName) : displayName;
            this.isReadOnly = isReadOnly;
        }

        public virtual void RefreshIsReadOnly() {
            this.RaisePropertyChanged(nameof(this.IsReadOnly));
        }
    }
}