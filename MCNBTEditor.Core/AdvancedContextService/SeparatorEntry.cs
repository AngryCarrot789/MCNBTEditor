namespace MCNBTEditor.Core.AdvancedContextService {
    /// <summary>
    /// A separator element between menu items
    /// </summary>
    public class SeparatorEntry : IContextEntry {
        public static SeparatorEntry Instance { get; } = new SeparatorEntry();
    }
}