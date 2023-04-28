namespace MCNBTEditor.Core.Explorer {
    public delegate void SelectionChangedEventHandler<in T>(T oldItem, T newItem);
}