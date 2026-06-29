using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace TileMatch3.Core.EffectSystem
{
    public class PooledVFX : MonoBehaviour
    {
        private IObjectPool<PooledVFX> _pool;
        public Animator _animator;
        public SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Lưu trữ tham chiếu đến Pool quản lý nó để nó có thể tự trả về
        public void SetPool(IObjectPool<PooledVFX> pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// Kích hoạt VFX và tự động trả về Pool sau một khoảng thời gian
        /// </summary>
        public async UniTaskVoid PlayAndAutoRelease(float lifeTime, Color mainColor)
        {
            if (_animator != null)
            {
                _animator.speed = 1 / lifeTime;
            }
            
            _spriteRenderer.color = mainColor;

            // Chờ VFX chạy xong (tương đương với việc đợi để Destroy như code cũ)
            await UniTask.Delay(TimeSpan.FromSeconds(lifeTime),
                cancellationToken: this.GetCancellationTokenOnDestroy());

            // Sau khi chạy xong, trả object này về lại Pool
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
                // Fallback nếu quên set pool
                Destroy(gameObject);
            }
        }
    }
}