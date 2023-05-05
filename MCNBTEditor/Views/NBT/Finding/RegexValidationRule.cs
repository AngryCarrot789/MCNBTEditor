using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace MCNBTEditor.Views.NBT.Finding {
    public class RegexValidationRule : ValidationRule {
        public bool IsEnabled { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (this.IsEnabled && value is string input) {
                try {
                    Regex.Match("_", input);
                    return ValidationResult.ValidResult;
                }
                catch {
                    return new ValidationResult(false, "Invalid regex expression: " + input);
                }
            }
            else {
                return ValidationResult.ValidResult;
            }
        }
    }
}
