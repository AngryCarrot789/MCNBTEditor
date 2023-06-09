using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MCNBTEditor.Core.Actions;
using MCNBTEditor.Utils;
using MCNBTEditor.Views.NBT;

namespace MCNBTEditor.Views {
    /// <summary>
    /// An extended window which adds support for a few of the things in the dark theme I made (e.g. Titlebar brush)
    /// </summary>
    public class WindowEx : Window {
        public static readonly DependencyProperty TitlebarBrushProperty = DependencyProperty.Register("TitlebarBrush", typeof(Brush), typeof(WindowEx), new PropertyMetadata());
        public static readonly DependencyProperty CanCloseWithEscapeKeyProperty = DependencyProperty.Register("CanCloseWithEscapeKey", typeof(bool), typeof(WindowEx), new PropertyMetadata(false));

        [Category("Brush")]
        public Brush TitlebarBrush {
            get => (Brush) this.GetValue(TitlebarBrushProperty);
            set => this.SetValue(TitlebarBrushProperty, value);
        }

        public bool CanCloseWithEscapeKey {
            get => (bool) this.GetValue(CanCloseWithEscapeKeyProperty);
            set => this.SetValue(CanCloseWithEscapeKeyProperty, value);
        }

        private bool isInRegularClosingHandler;
        private bool isHandlingAsyncClose;

        private readonly Action showAction;
        private readonly Func<bool?> showDialogAction;

        public WindowEx() : base() {
            this.showAction = this.Show;
            this.showDialogAction = this.ShowDialog;
        }

        public Task ShowAsync() {
            // Just in case this is called off the main thread
            return DispatcherUtils.InvokeAsync(this.Dispatcher, this.showAction);
        }

        public Task<bool?> ShowDialogAsync() {
            return DispatcherUtils.InvokeAsync(this.Dispatcher, this.showDialogAction);
        }

        protected sealed override async void OnClosing(CancelEventArgs e) {
            if (this.isInRegularClosingHandler || this.isHandlingAsyncClose) {
                return;
            }

            try {
                this.isInRegularClosingHandler = true;
                e.Cancel = true;
                if (await this.CloseAsync()) {
                    e.Cancel = false;
                }
            }
            finally {
                this.isInRegularClosingHandler = false;
            }
        }

        /// <summary>
        /// Closes the window
        /// </summary>
        /// <returns>Whether the window was closed or not</returns>
        public Task<bool> CloseAsync() {
            // return await await Task.Run(async () => await DispatcherUtils.InvokeAsync(this.Dispatcher, this.CloseAsyncInternal));
            return DispatcherUtils.Invoke(this.Dispatcher, this.CloseAsyncInternal);
        }

        private async Task<bool> CloseAsyncInternal() {
            if (await this.OnClosingAsync()) {
                if (this.isInRegularClosingHandler) {
                    return true;
                }

                try {
                    this.isHandlingAsyncClose = true;
                    await DispatcherUtils.InvokeAsync(this.Dispatcher, this.Close);
                    return true;
                }
                finally {
                    this.isHandlingAsyncClose = false;
                }
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Called when the window is trying to be closed
        /// </summary>
        /// <returns>True if the window can close, otherwise false to stop it from closing</returns>
        protected virtual Task<bool> OnClosingAsync() {
            return Task.FromResult(true);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            base.OnPreviewKeyDown(e);
            if (e.Handled) {
                return;
            }

            if (e.Key == Key.Escape && this.CanCloseWithEscapeKey) {
                e.Handled = true;
                this.Close();
            }
        }

        // [ActionRegistration("actions.views.windows.CloseViewAction")]
        // private class CloseViewAction : AnAction {
        //     public CloseViewAction() : base(() => "Close window", () => "Closes the current window") {
        //     }
        //     public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
        //         if (e.DataContext.TryGetContext(out WindowEx w) && w.CanCloseWithEscapeKey) {
        //             await w.CloseAsync();
        //             return true;
        //         }
        //         return false;
        //     }
        //     public override Presentation GetPresentation(AnActionEventArgs e) {
        //         return Presentation.BoolToEnabled(e.DataContext.TryGetContext<WindowEx>(out _));
        //     }
        // }


        // Binding a checkbox to the window's Topmost property is more effective and works both ways
        [ActionRegistration("actions.views.MakeWindowTopMost")]
        private class MakeTopMostAction : ToggleAction {
            public MakeTopMostAction() : base(() => "Make window top-most", () => "Makes the window top most, so that non-top-most windows cannot be on top of it") {

            }

            public override Task<bool> OnToggled(AnActionEventArgs e, bool isToggled) {
                if (e.DataContext.TryGetContext(out WindowEx window)) {
                    window.Topmost = isToggled;
                    return Task.FromResult(true);
                }
                else {
                    return Task.FromResult(false);
                }
            }

            public override Task<bool> ExecuteNoToggle(AnActionEventArgs e) {
                if (e.DataContext.TryGetContext(out WindowEx window)) {
                    window.Topmost = !window.Topmost;
                    return Task.FromResult(true);
                }
                else {
                    return Task.FromResult(false);
                }
            }

            public override Presentation GetPresentation(AnActionEventArgs e) {
                return Presentation.BoolToEnabled(e.DataContext.TryGetContext<WindowEx>(out _));
            }
        }
    }
}