using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCNBTEditor.Core.Actions.Helpers {
    /// <summary>
    /// Creates a new action which invokes an <see cref="ICommand"/>
    /// </summary>
    /// <typeparam name="T">The type which contains the target command</typeparam>
    public class CommandAction<T> : AnAction {
        /// <summary>
        /// The function that gets the <see cref="ICommand"/> instance from an object.
        /// The function will not be null, but invoking it may return a null value
        /// </summary>
        public Func<T, ICommand> CommandAccessor { get; }

        public bool ResultWhenNullCommand { get; set; } = false;
        public bool ResultWhenCannotExecute { get; set; } = false;
        public bool ResultWhenExecuteSuccess { get; set; } = true;

        public Presentation PresentationWhenNullCommand { get; set; } = Presentation.VisibleAndDisabled;
        public Presentation PresentationWhenCannotExecute { get; set; } = Presentation.VisibleAndDisabled;
        public Presentation PresentationWhenCanExecute { get; set; } = Presentation.VisibleAndEnabled;

        /// <summary>
        /// Constructor for <see cref="CommandAction{T}"/>
        /// </summary>
        /// <param name="propertyName">The name of the <see cref="ICommand"/> property in <see cref="T"/></param>
        /// <exception cref="ArgumentNullException">Null property name</exception>
        /// <exception cref="Exception">No such property in <see cref="T"/> named by <see cref="propertyName"/></exception>
        public CommandAction(string propertyName) : base() {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (propertyInfo == null) {
                throw new Exception($"No such property: {typeof(T)}.{propertyName}");
            }

            this.CommandAccessor = (Func<T, ICommand>) Delegate.CreateDelegate(typeof(Func<T, ICommand>), propertyInfo.GetMethod);
        }

        /// <summary>
        /// Constructor for <see cref="CommandAction{T}"/> which accepts a non-null function for getting an <see cref="ICommand"/> from an instance of <see cref="T"/>
        /// </summary>
        /// <param name="accessor">The command getter function</param>
        /// <exception cref="ArgumentNullException">Null function</exception>
        public CommandAction(Func<T, ICommand> accessor) : base() {
            this.CommandAccessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out T instance)) {
                return false;
            }

            ICommand cmd = this.CommandAccessor(instance);
            if (cmd == null) {
                return this.ResultWhenNullCommand;
            }

            if (!cmd.CanExecute(null)) {
                return this.ResultWhenCannotExecute;
            }

            if (cmd is BaseAsyncRelayCommand asyncCmd) {
                await asyncCmd.ExecuteAsync(null);
            }
            else {
                cmd.Execute(null);
            }

            return this.ResultWhenExecuteSuccess;
        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out T instance)) {
                return Presentation.Invisible;
            }

            ICommand cmd = this.CommandAccessor(instance);
            if (cmd == null) {
                return this.PresentationWhenNullCommand;
            }

            if (cmd.CanExecute(null)) {
                return this.PresentationWhenCanExecute;
            }

            return this.PresentationWhenCannotExecute;
        }
    }
}