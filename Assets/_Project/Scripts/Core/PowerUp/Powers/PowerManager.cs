using System.Collections.Generic;
using ImprovedTimers;
using UnityEngine;

namespace TileMatch3.Core.PowerUp
{
    public class PowerManager : MonoBehaviour
    {
        private const float UPDATE_INTERVAL = 0.02f; 
        [SerializeField] private float cooldown = 1f;
        
        private List<IPower> powers = new();
        private IntervalTimer waitingIntervalTimer;

        private float waitingProgress;

        private void Start()
        {
            waitingIntervalTimer = new IntervalTimer(cooldown, UPDATE_INTERVAL);
            waitingIntervalTimer.OnTimerStart += OnPowersStartWaiting;
            waitingIntervalTimer.OnTimerStop += OnPowersStopWaiting;
            waitingIntervalTimer.OnInterval += OnPowersProcessWaiting;
        }

        private void OnPowersStartWaiting()
        {
            waitingProgress = 0f;
            powers.ForEach(power => power.OnStartWaiting());
        }

        private void OnPowersProcessWaiting()
        {
            waitingProgress += UPDATE_INTERVAL;
            powers.ForEach(OnEachPowerProcessWaiting);
        }

        private void OnEachPowerProcessWaiting(IPower power)
        {
            power.OnProcessWaiting(waitingProgress);
        }

        private void OnPowersStopWaiting()
        {
            powers.ForEach(power => power.OnStopWaiting());
        }

        private void OnDestroy()
        {
            waitingIntervalTimer.OnTimerStart -= OnPowersStartWaiting;
            waitingIntervalTimer.OnTimerStop -= OnPowersStopWaiting;
            waitingIntervalTimer.OnInterval -= OnPowersProcessWaiting;
            waitingIntervalTimer = null;
        }

        public void AddPower(IPower power) => powers.Add(power);
        public void RemovePower(IPower power) => powers.Remove(power);

        public void OnAnyPowerActivated()
        {
            waitingIntervalTimer.Reset();
            waitingIntervalTimer.Start();
        }
    }
}