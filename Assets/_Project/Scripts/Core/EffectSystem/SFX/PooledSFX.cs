using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace TileMatch3.Core.EffectSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class PooledSFX : MonoBehaviour
    {
        private IObjectPool<PooledSFX> _pool;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            
            // Tối ưu hoá cho game 2D
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f; // 0 = 2D Sound, 1 = 3D Sound
        }

        public void SetPool(IObjectPool<PooledSFX> pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// Kích hoạt SFX và tự động trả về Pool sau khi audio chạy xong
        /// </summary>
        public async UniTaskVoid PlayAndAutoRelease(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
            {
                ReleaseToPool();
                return;
            }

            _audioSource.clip = clip;
            _audioSource.volume = volume;
            _audioSource.Play();

            // Chờ audio chạy xong dựa trên độ dài của AudioClip
            await UniTask.Delay(TimeSpan.FromSeconds(clip.length), cancellationToken: this.GetCancellationTokenOnDestroy());

            ReleaseToPool();
        }

        private void ReleaseToPool()
        {
            if (_pool != null)
            {
                _pool.Release(this);
            }
            else
            {
                Destroy(gameObject); 
            }
        }
    }
}