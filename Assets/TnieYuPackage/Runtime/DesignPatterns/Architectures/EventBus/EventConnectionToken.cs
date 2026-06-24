using System;

namespace TnieYuPackage.DesignPatterns
{
    public readonly struct EventConnectionToken<TEvent> : IDisposable
        where TEvent : IEventData
    {
        private readonly IEventSubscriber<TEvent> subscriber;

        public EventConnectionToken(IEventSubscriber<TEvent> subscriber)
        {
            this.subscriber = subscriber;
        }

        public void Dispose()
        {
            if (subscriber == null) return;
            
            GlobalEventBus.UnregisterHandler(subscriber);
        }
    }
}