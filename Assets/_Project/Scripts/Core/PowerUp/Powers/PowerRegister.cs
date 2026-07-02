using System;
using Obvious.Soap;
using Reflex.Attributes;
using SerializableInterface.Runtime;
using UnityEngine;

namespace TileMatch3.Core.PowerUp
{
    public class PowerRegister : MonoBehaviour
    {
        [SerializeField] private InterfaceReference<IPower> power;
        [Inject] private PowerManager powerManager;

        [SerializeField] private IntVariable powerRemainingVariable;

        private void Awake()
        {
            if (power == null) return;

            powerRemainingVariable.OnValueChanged += OnPowerRemainingValueChanged;

            power.Value.OnClick += OnPowerClick;

            if (powerManager != null)
            {
                powerManager.AddPower(power.Value);
            }
        }

        private void Start()
        {
            OnPowerRemainingValueChanged(powerRemainingVariable.Value);
        }

        private void OnPowerRemainingValueChanged(int newValue)
        {
            if (newValue > 0)
            {
                power.Value.IsEnoughEnable = true;
                if (power.Value.IsDoubleCheck)
                    power.Value.SetPowerOn();
            }
            else
            {
                power.Value.IsEnoughEnable = false;
                power.Value.SetPowerOff();
            }
        }

        private void OnPowerClick()
        {
            powerRemainingVariable.Value -= 1;
            powerManager.OnAnyPowerActivated();
        }

        private void OnDestroy()
        {
            if (power == null) return;

            power.Value.OnClick -= OnPowerClick;
            powerRemainingVariable.OnValueChanged -= OnPowerRemainingValueChanged;

            if (powerManager != null)
            {
                powerManager.RemovePower(power.Value);
            }
        }
    }
}