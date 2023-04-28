using System.Threading.Tasks;
using MCNBTEditor.Core.Views.Dialogs.UserInputs;
using MCNBTEditor.Utils;

namespace MCNBTEditor.Views.UserInputs {
    public class UserInputDialogService : IUserInputDialogService {
        public async Task<string> ShowSingleInputDialog(string title = "Input a value", string message = "Input a new valid", string def = null, InputValidator validator = null) {
            SingleInputViewModel vm = new SingleInputViewModel() {
                Title = title,
                Message = message,
                Input = def,
                ValidateInput = validator
            };

            return await this.ShowSingleInputDialog(vm) ? vm.Input ?? "" : null;
        }

        public Task<bool> ShowSingleInputDialog(SingleInputViewModel viewModel) {
            SingleUserInputWindow window = new SingleUserInputWindow {
                DataContext = viewModel
            };

            viewModel.Dialog = window;
            if (viewModel.ValidateInput != null && window.InputValidationRule != null) {
                window.InputValidationRule.Validator = viewModel.ValidateInput;
            }

            return DispatcherUtils.InvokeAsync(window.Dispatcher, () => window.ShowDialog() == true);
        }

        public Task<bool> ShowDoubleInputDialog(DoubleInputViewModel viewModel) {
            DoubleUserInputWindow window = new DoubleUserInputWindow {
                DataContext = viewModel
            };

            viewModel.Dialog = window;
            if (viewModel.ValidateInputA != null && window.InputValidationRuleA != null)
                window.InputValidationRuleA.Validator = viewModel.ValidateInputA;
            if (viewModel.ValidateInputB != null && window.InputValidationRuleB != null)
                window.InputValidationRuleB.Validator = viewModel.ValidateInputB;
            return DispatcherUtils.InvokeAsync(window.Dispatcher, () => window.ShowDialog() == true);
        }
    }
}