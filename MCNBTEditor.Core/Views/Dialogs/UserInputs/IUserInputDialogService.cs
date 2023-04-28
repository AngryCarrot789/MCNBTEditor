using System.Threading.Tasks;

namespace MCNBTEditor.Core.Views.Dialogs.UserInputs {
    public interface IUserInputDialogService {
        // TODO: Convert to async
        Task<string> ShowSingleInputDialog(string title = "Input a value", string message = "Input a new valid", string def = null, InputValidator validator = null);
        Task<bool> ShowSingleInputDialog(SingleInputViewModel viewModel);
        Task<bool> ShowDoubleInputDialog(DoubleInputViewModel viewModel);
    }
}