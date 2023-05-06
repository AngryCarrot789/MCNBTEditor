using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCNBTEditor.Core.Actions.Helpers {
    /// <summary>
    /// Creates a new action which invokes an <see cref="ICommand"/>
    /// </summary>
    /// <typeparam name="T">The type which contains the target command</typeparam>
    public class ShortcutActionCommand<T> : AnAction {
        public Func<T, ICommand> CommandAccessor { get; }

        public string ShortcutId { get; }

        /// <summary>
        /// Constructor for <see cref="ShortcutActionCommand{T}"/>
        /// </summary>
        /// <param name="shortcutId">The ID of the shortcut</param>
        /// <param name="propertyName">The name of the <see cref="ICommand"/> property in <see cref="T"/></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public ShortcutActionCommand(string shortcutId, string propertyName) : base((string) null, null) {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (propertyInfo == null) {
                throw new Exception($"No such property: {typeof(T)}.{propertyName}");
            }

            this.CommandAccessor = (Func<T, ICommand>) Delegate.CreateDelegate(typeof(Func<T, ICommand>), propertyInfo.GetMethod);
            this.ShortcutId = shortcutId;
        }

        public ICommand GetCommand(in T instance) {
            return this.CommandAccessor(instance);
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out T instance)) {
                return false;
            }

            ICommand cmd = this.GetCommand(instance);
            if (cmd == null || !cmd.CanExecute(null)) {
                return false;
            }

            if (cmd is BaseAsyncRelayCommand asyncCmd) {
                await asyncCmd.ExecuteAsync(null);
            }
            else {
                cmd.Execute(null);
            }

            return true;
        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out T instance)) {
                return Presentation.Invisible;
            }

            ICommand cmd = this.GetCommand(instance);
            if (cmd == null || !cmd.CanExecute(null)) {
                return Presentation.VisibleAndDisabled;
            }

            return Presentation.VisibleAndEnabled;
        }
    }
}