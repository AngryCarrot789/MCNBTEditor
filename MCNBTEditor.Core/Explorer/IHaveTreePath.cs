namespace MCNBTEditor.Core.Explorer {
    public interface IHaveTreePath {
        /// <summary>
        /// Returns this instance's path part
        /// </summary>
        string TreePathPartName { get; }
    }
}