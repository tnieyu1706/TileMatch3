using System;
using System.Collections.Generic;

namespace TnieYuPackage.DesignPatterns
{
    #region Configuration

    public interface IEventData
    {
    }

    public interface IEventSubscriber
    {
        public bool IsDisposed { get; }

        public void Handle(IEventData data);
    }

    public interface IEventSubscriber<in TEvent> : IEventSubscriber
        where TEvent : IEventData
    {
        void IEventSubscriber.Handle(IEventData data)
        {
            if (data is TEvent e) Handle(e);
        }

        public void Handle(TEvent e);
    }

    #endregion

    public static class GlobalEventBus
    {
        private static readonly Dictionary<Type, List<IEventSubscriber>> Subscribers = new();

        public static event Action<IEventSubscriber> OnSubscriberRegistered;
        public static event Action<IEventSubscriber> OnSubscriberUnregistering;

        #region Sub

        public static IDisposable RegisterHandler<TEvent>(IEventSubscriber<TEvent> subscriber)
            where TEvent : IEventData
        {
            var eventType = typeof(TEvent);

            if (!Subscribers.TryGetValue(eventType, out var list))
            {
                list = new List<IEventSubscriber>();
                Subscribers[eventType] = list;
            }

            list.Add(subscriber);
            OnSubscriberRegistered?.Invoke(subscriber);

            return new EventConnectionToken<TEvent>(subscriber);
        }

        public static void UnregisterHandler<TEvent>(IEventSubscriber<TEvent> subscriber)
            where TEvent : IEventData
        {
            if (subscriber == null || subscriber.IsDisposed) return;

            var eventType = typeof(TEvent);

            if (!Subscribers.TryGetValue(eventType, out var list)) return;

            OnSubscriberUnregistering?.Invoke(subscriber);
            list.Remove(subscriber);

            if (list.Count == 0)
            {
                Subscribers.Remove(eventType);
            }
        }

        #endregion

        #region Pub

        public static void Publish(IEventData eventData)
        {
            if (eventData == null) return;

            var eventType = eventData.GetType();
            if (!Subscribers.TryGetValue(eventType, out var subscribers))
            {
                return;
            }

            var snapshot = subscribers.ToArray();

            foreach (var subscriber in snapshot)
            {
                subscriber.Handle(eventData);
            }
        }

        #endregion
    }
}