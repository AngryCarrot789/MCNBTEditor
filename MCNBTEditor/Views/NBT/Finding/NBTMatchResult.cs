using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using MCNBTEditor.Core;
using MCNBTEditor.Core.AdvancedContextService;
using MCNBTEditor.Core.Explorer.NBT;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.Views.NBT.Finding {
    public class NBTMatchResult : IContextProvider {
        public BaseTagViewModel NBT { get; }

        public string NameSearchTerm { get; }

        public string ValueSearchTerm { get; }

        public string PrimitiveOrArrayFoundValue { get; }

        public List<TextRange> NameMatches { get; }

        public List<TextRange> ValueMatches { get; }

        public ICommand NavigateToItemCommand { get; }

        public NBTMatchResult(BaseTagViewModel nbt, string nameSearchTerm, string valueSearchTerm, string primitiveOrArrayFoundValue, IEnumerable<Match> nameMatches, IEnumerable<Match> valueMatches) :
            this(nbt, nameSearchTerm, valueSearchTerm, primitiveOrArrayFoundValue, nameMatches.Select(x => new TextRange(x.Index, x.Length)), valueMatches.Select(x => new TextRange(x.Index, x.Length))){
        }

        public NBTMatchResult(BaseTagViewModel nbt, string nameSearchTerm, string valueSearchTerm, string primitiveOrArrayFoundValue, IEnumerable<TextRange> nameMatches, IEnumerable<TextRange> valueMatches) :
            this(nbt, nameSearchTerm, valueSearchTerm, primitiveOrArrayFoundValue, nameMatches != null ? nameMatches.ToList() : new List<TextRange>(), valueMatches != null ? valueMatches.ToList() : new List<TextRange>()){
        }

        public NBTMatchResult(BaseTagViewModel nbt, string nameSearchTerm, string valueSearchTerm, string primitiveOrArrayFoundValue, List<TextRange> nameMatches, List<TextRange> valueMatches) {
            this.NBT = nbt;
            this.NameSearchTerm = nameSearchTerm;
            this.ValueSearchTerm = valueSearchTerm;
            this.PrimitiveOrArrayFoundValue = primitiveOrArrayFoundValue;
            this.NameMatches = nameMatches ?? new List<TextRange>();
            this.ValueMatches = valueMatches ?? new List<TextRange>();
            this.NavigateToItemCommand = new AsyncRelayCommand(this.NavigateToItemAction);
        }

        private async Task NavigateToItemAction() {
            await IoC.TreeView.RepeatExpandHierarchyFromRootAsync(this.NBT.GetParentChain(false));
        }

        public void GetContext(List<IContextEntry> list) {
            list.Add(new CommandContextEntry("Navigate", this.NavigateToItemCommand));
            list.Add(SeparatorEntry.Instance);
            this.NBT.GetContext(list);
        }
    }
}