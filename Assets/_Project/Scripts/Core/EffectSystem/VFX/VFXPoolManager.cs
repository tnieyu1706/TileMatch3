using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.Pool;

namespace TileMatch3.Core.EffectSystem
{
    /// <summary>
    /// Manager này dùng Singleton để dễ dàng gọi ở bất cứ đâu.
    /// Hỗ trợ quản lý nhiều loại Prefab VFX khác nhau.
    /// </summary>
    public class VFXPoolManager : Singleton<VFXPoolManager>
    {
        // Dictionary lưu trữ các Pool khác nhau dựa trên Prefab gốc
        private Dictionary<GameObject, ObjectPool<PooledVFX>> _pools = new();

        /// <summary>
        /// Gọi hàm này để lấy ra một VFX tại vị trí mong muốn
        /// </summary>
        public PooledVFX SpawnVFX(GameObject prefab, Vector3 position, float lifeTime, Color? color)
        {
            if (prefab == null) return null;

            // Nếu loại VFX này chưa có Pool, tạo một Pool mới cho nó
            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = CreateNewPool(prefab);
                _pools[prefab] = pool;
            }

            // Lấy VFX ra từ Pool
            PooledVFX vfxInstance = pool.Get();

            // Cập nhật vị trí
            vfxInstance.transform.position = position;

            // Phát và hẹn giờ tự động thu hồi
            vfxInstance.PlayAndAutoRelease(lifeTime, color).Forget();

            return vfxInstance;
        }

        private ObjectPool<PooledVFX> CreateNewPool(GameObject prefab)
        {
            return new ObjectPool<PooledVFX>(
                createFunc: () =>
                {
                    // Hàm tạo mới Object khi Pool trống
                    GameObject obj = Instantiate(prefab, transform); // Đặt làm con của Manager cho gọn Hierarchy

                    // Đảm bảo Prefab có script PooledVFX
                    PooledVFX pooledVfx = obj.GetComponent<PooledVFX>();
                    if (pooledVfx == null)
                    {
                        pooledVfx = obj.AddComponent<PooledVFX>();
                    }

                    return pooledVfx;
                },
                actionOnGet: (vfx) =>
                {
                    // Khi lấy ra khỏi Pool: Bật nó lên
                    vfx.gameObject.SetActive(true);
                },
                actionOnRelease: (vfx) =>
                {
                    // Khi trả về Pool: Tắt nó đi
                    vfx.gameObject.SetActive(false);
                },
                actionOnDestroy: (vfx) =>
                {
                    // Khi Pool quá đầy và cần xoá bớt
                    Destroy(vfx.gameObject);
                },
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 50 // Giới hạn tối đa số lượng VFX loại này tồn tại cùng lúc
            );
        }

        // Helper để các PooledVFX nhận diện đúng Pool của mình khi được sinh ra
        public void AssignPoolToInstance(GameObject prefab, PooledVFX instanceSource)
        {
            if (_pools.TryGetValue(prefab, out var pool))
            {
                instanceSource.SetPool(pool);
            }
        }
    }
}