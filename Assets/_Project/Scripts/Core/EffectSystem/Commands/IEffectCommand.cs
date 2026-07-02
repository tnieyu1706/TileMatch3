using System;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace TileMatch3.Core.EffectSystem.Commands
{
    public interface IEffectCommand
    {
        void Execute(Vector3 position, float duration, Color? color);
    }

    [Serializable]
    public class VFXCommand : IEffectCommand
    {
        [Tooltip("Prefab VFX cần sinh ra")] public GameObject vfxPrefab;

        // Không cần lưu trữ state (_position, _duration...) bên trong class nữa
        public void Execute(Vector3 position, float duration, Color? color)
        {
            if (vfxPrefab != null && Singleton<VFXPoolManager>.Instance != null)
            {
                PooledVFX vfx = Singleton<VFXPoolManager>.Instance.SpawnVFX(vfxPrefab, position, duration, color);
                Singleton<VFXPoolManager>.Instance.AssignPoolToInstance(vfxPrefab, vfx);
            }
        }
    }

    [Serializable]
    public class SFXCommand : IEffectCommand
    {
        [Tooltip("Âm thanh cần phát")] public AudioClip sfxClip;

        [Range(0f, 1f)] public float volume = 1f;

        public void Execute(Vector3 position, float duration, Color? color)
        {
            if (sfxClip != null && Singleton<SFXPoolManager>.Instance != null)
            {
                Singleton<SFXPoolManager>.Instance.PlaySFX(sfxClip, position, volume);
            }
        }
    }
}