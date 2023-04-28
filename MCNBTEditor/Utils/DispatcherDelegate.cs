using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using MCNBTEditor.Core.Services;

namespace MCNBTEditor.Utils {
    public class DispatcherDelegate : IDispatcher {
        private readonly Dispatcher dispatcher;

        public bool IsOnOwnerThread => this.dispatcher.CheckAccess();

        public DispatcherDelegate(Dispatcher dispatcher) {
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher), "Dispatcher cannot be null");
        }

        public void Invoke(Action action) {
            this.dispatcher.Invoke(action);
        }

        public void InvokeLater(Action action, bool background = false) {
            this.dispatcher.Invoke(action, background ? DispatcherPriority.Background : DispatcherPriority.Normal);
        }

        public T Invoke<T>(Func<T> function) {
            return this.dispatcher.Invoke(function);
        }

        public T InvokeLater<T>(Func<T> function, bool background = false) {
            return this.dispatcher.Invoke(function, background ? DispatcherPriority.Background : DispatcherPriority.Normal);
        }

        public Task InvokeAsync(Action action) {
            return DispatcherUtils.InvokeAsync(this.dispatcher, action);
        }

        public Task InvokeLaterAsync(Action action, bool background = false) {
            return this.dispatcher.InvokeAsync(action, background ? DispatcherPriority.Background : DispatcherPriority.Normal).Task;
        }

        public Task<T> InvokeAsync<T>(Func<T> function) {
            return DispatcherUtils.InvokeAsync(this.dispatcher, function);
        }

        public Task<T> InvokeLaterAsync<T>(Func<T> function, bool background = false) {
            return this.dispatcher.InvokeAsync(function, background ? DispatcherPriority.Background : DispatcherPriority.Normal).Task;
        }
    }
}