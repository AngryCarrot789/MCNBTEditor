namespace MCNBTEditor.Core.Explorer {
    public interface IHaveTreePath {
        /// <summary>
        /// Returns this pathable instance's name
        /// </summary>
        string TreePathPartName { get; }
    }
}