using System.Collections.Generic;
using System.Windows;
using MCNBTEditor.Core.Actions.Contexts;
using MCNBTEditor.Core.AdvancedContextService;

namespace MCNBTEditor.AdvancedContextService {
    public interface IWPFContextGenerator {
        /// <summary>
        /// Generates context entries and adds them into the list parameter. Leading, repeated and trailing separators are automatically filtered out
        /// </summary>
        /// <param name="list">The list in which entries should be added to</param>
        /// <param name="sender">The control whose context menu is being opened (typically the one whose generator property is set in XAML)</param>
        /// <param name="target">The actual target element which was clicked. This is a visual child of the sender parameter</param>
        /// <param name="context">The data context of the target element (for convenience)</param>
        void Generate(List<IContextEntry> list, DependencyObject sender, DependencyObject target, object context);
    }
}