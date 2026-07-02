using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.Pool;

namespace TileMatch3.Core.EffectSystem
{
    /// <summary>
    /// Manager quản lý toàn bộ SFX. Chỉ cần 1 Pool chung cho tất cả các loại AudioClip.
    /// </summary>
    public class SFXPoolManager : Singleton<SFXPoolManager>
    {
        [SerializeField] private PooledSFX pooledSfxPrefab;

        private ObjectPool<PooledSFX> _pool;

        protected override void Awake()
        {
            base.Awake();
            InitializePool();
        }

        private void InitializePool()
        {
            _pool = new ObjectPool<PooledSFX>(
                createFunc: () =>
                {
                    PooledSFX pooledSfx;
                    if (pooledSfxPrefab != null)
                    {
                        pooledSfx = Instantiate(pooledSfxPrefab, transform);
                    }
                    else
                    {
                        // Tạo một GameObject rỗng và gắn script PooledSFX (tự động có thêm AudioSource)
                        GameObject obj = new GameObject("PooledSFX_Instance");
                        obj.transform.SetParent(transform); // Đặt làm con của Manager

                        pooledSfx = obj.AddComponent<PooledSFX>();
                    }

                    pooledSfx.SetPool(_pool);
                    return pooledSfx;
                },
                actionOnGet: (sfx) => { sfx.gameObject.SetActive(true); },
                actionOnRelease: (sfx) => { sfx.gameObject.SetActive(false); },
                actionOnDestroy: (sfx) => { Destroy(sfx.gameObject); },
                collectionCheck: true,
                defaultCapacity: 15,
                maxSize: 50 // Giới hạn số lượng âm thanh phát ra cùng lúc
            );
        }

        /// <summary>
        /// Gọi hàm này để phát một AudioClip tại vị trí mong muốn
        /// </summary>
        public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;

            // Lấy SFX từ Pool ra
            PooledSFX sfxInstance = _pool.Get();

            // Thiết lập vị trí
            sfxInstance.transform.position = position;

            // Phát và tự thu hồi
            sfxInstance.PlayAndAutoRelease(clip, volume).Forget();
        }
    }
}