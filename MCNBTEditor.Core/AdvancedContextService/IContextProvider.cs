using System.Collections.Generic;

namespace MCNBTEditor.Core.AdvancedContextService {
    public interface IContextProvider {
        void GetContext(List<IContextEntry> list);
    }
}