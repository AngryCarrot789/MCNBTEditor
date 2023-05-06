using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MCNBTEditor.Core.Timing {
    public class IdleEventService : IDisposable {
        public delegate void BeginActionEvent();
        public event BeginActionEvent OnIdle;

        private DateTime lastInput;
        private volatile bool canFireEvent;
        private volatile bool stopTask;

        public TimeSpan MinimumTimeSinceInput { get; set; }

        /// <summary>
        /// The amount of time between ticks that the underlying task will try to adhere by. Too low of a value may eat up CPU
        /// </summary>
        public TimeSpan TaskTickInterval { get; set; }

        public bool CanFireNextTick {
            get => this.canFireEvent;
            set => this.canFireEvent = value;
        }

        public IdleEventService() {
            this.MinimumTimeSinceInput = TimeSpan.FromMilliseconds(200);
            this.TaskTickInterval = TimeSpan.FromMilliseconds(100);
            this.Start();
        }

        private void Start() {
            Task.Run(async () => {
                while (!this.stopTask) {
                    if ((DateTime.Now - this.lastInput) > this.MinimumTimeSinceInput && this.canFireEvent) {
                        this.canFireEvent = false;
                        if (this.stopTask) {
                            break;
                        }

                        try {
                            await IoC.Dispatcher.InvokeAsync(this.FireEvent);
                        }
                        catch (ThreadAbortException) {
                            return;
                        }
                        catch (Exception e) {
                            Debug.WriteLine(e.ToString());
                            #if DEBUG
                            throw;
                            #else
                            await IoC.MessageDialogs.ShowMessageAsync("Error", "An error occurred during an idle event service: " + e.Message);
                            #endif
                        }
                    }

                    await Task.Delay(this.TaskTickInterval);
                }
            });
        }

        public void FireEvent() {
            this.OnIdle?.Invoke();
        }

        public void OnInput() {
            this.canFireEvent = true;
            this.lastInput = DateTime.Now;
        }

        public void ForceAction() {
            this.canFireEvent = false;
            this.lastInput = DateTime.Now;
            this.FireEvent();
        }

        public void Dispose() {
            this.stopTask = true;
        }
    }
}