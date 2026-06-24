using System;
using System.Collections.Generic;
using System.Threading;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace TnieYuPackage.Core
{
    [DefaultExecutionOrder(-1000)]
    public class EventManager : PersistentSingleton<EventManager>
    {
        private readonly Queue<Action> queue = new();

        void Update()
        {
            while (queue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        private void OnDestroy()
        {
            queue.Clear();
        }

        public void Registry(Action action)
        {
            queue.Enqueue(action);
        }
        
        public async Awaitable RegistryDelay(Action action, float delay, CancellationToken token = default)
        {
            // await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
            await Awaitable.WaitForSecondsAsync(delay, token);
            Registry(action);
        }
    }
}