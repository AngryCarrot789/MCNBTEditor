namespace MCNBTEditor.Core.Explorer {
    public interface IHaveChildren {
        void AddItem(BaseTreeItemViewModel item);
        bool RemoveItem(BaseTreeItemViewModel item);
        int IndexOfItem(BaseTreeItemViewModel item);

        void ClearItems();
    }
}