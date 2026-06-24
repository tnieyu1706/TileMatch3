using System;
using System.Collections.Generic;

namespace TnieYuPackage.DesignPatterns
{
    public interface IEventBus
    {
        IDisposable RegisterSub<TEvent>(IEventSubscriber<TEvent> subscriber) where TEvent : IEventData;
        void UnregisterHandler<TEvent>(IEventSubscriber<TEvent> subscriber) where TEvent : IEventData;
        void Pub(IEventData eventData);
    }

    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<IEventSubscriber>> subscribers = new();

        public IDisposable RegisterSub<TEvent>(IEventSubscriber<TEvent> subscriber)
            where TEvent : IEventData
        {
            var eventType = typeof(TEvent);

            if (!subscribers.TryGetValue(eventType, out var list))
            {
                list = new List<IEventSubscriber>();
                subscribers[eventType] = list;
            }

            list.Add(subscriber);

            return new EventConnectionToken<TEvent>(subscriber);
        }

        public void UnregisterHandler<TEvent>(IEventSubscriber<TEvent> subscriber) where TEvent : IEventData
        {
            if (subscriber == null || subscriber.IsDisposed) return;

            var eventType = typeof(TEvent);

            if (!subscribers.TryGetValue(eventType, out var list))
            {
                return;
            }

            list.Remove(subscriber);

            if (list.Count == 0)
            {
                subscribers.Remove(eventType);
            }
        }

        public void Pub(IEventData eventData)
        {
            if (eventData == null) return;

            var eventType = eventData.GetType();
            if (!this.subscribers.TryGetValue(eventType, out var eventSubs))
            {
                return;
            }

            var snapshot = eventSubs.ToArray();

            foreach (var subscriber in snapshot)
            {
                subscriber.Handle(eventData);
            }
        }
    }
}