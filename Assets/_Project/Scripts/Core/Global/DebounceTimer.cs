using System;
using ImprovedTimers;
using UnityEngine;

namespace TileMatch3.Core.Global {
    /// <summary>
    /// Executes after there has been no activity for a specified duration.
    /// Calling Reset() (or Touch()) restarts the debounce window.
    /// After firing, it automatically starts waiting for the next idle period.
    /// </summary>
    public sealed class DebounceTimer : Timer {
        public event Action OnTick = delegate { };
        private float debounceTime;

        public DebounceTimer(float debounceTime) : base(0) {
            this.debounceTime = debounceTime;
        }

        public override void Tick() {
            if (!IsRunning)
                return;

            CurrentTime += Time.deltaTime;

            if (CurrentTime < debounceTime)
                return;

            CurrentTime -= debounceTime;
            OnTick.Invoke();
        }

        public override bool IsFinished => !IsRunning;

        public void SetDebounceTime(float seconds) {
            debounceTime = seconds;
        }
    }
}