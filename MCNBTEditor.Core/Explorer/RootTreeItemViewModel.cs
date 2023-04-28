using System;
using System.Collections.Generic;

namespace MCNBTEditor.Core.Explorer {
    public class RootTreeItemViewModel : BaseTreeItemViewModel, IHaveTreePath, IHaveChildren {
        public override bool CanHoldChildren => true;

        public string TreePathPartName => ""; // unix style >:)

        public RootTreeItemViewModel() {

        }

        public void AddItem(BaseTreeItemViewModel item) {
            base.Add(item);
        }

        public bool RemoveItem(BaseTreeItemViewModel item) {
            return base.Remove(item);
        }

        public int IndexOfItem(BaseTreeItemViewModel item) {
            return base.IndexOf(item);
        }

        public void ClearItems() {
            base.Clear();
        }
    }
}