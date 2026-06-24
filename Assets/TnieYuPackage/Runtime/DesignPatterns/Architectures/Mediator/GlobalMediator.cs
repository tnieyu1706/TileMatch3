using System;
using System.Collections.Generic;

namespace TnieYuPackage.DesignPatterns
{
    #region Configuration

    public readonly struct MediatorConnectionToken : IDisposable
    {
        private IComponent Component { get; }

        public MediatorConnectionToken(IComponent component)
        {
            Component = component;
        }
        
        public void Dispose()
        {
            if (Component == null) return;
            GlobalMediator.Disconnect(Component);
        }
    }

    #endregion
    
    /// <summary>
    /// Mediator (controller) is Distributor Center support transfer message
    /// Level 1: Normal Controller
    /// Level 2: Message Mediator
    /// Level 3: Message is Visitor (more Flexible)
    /// Current Mediator at [Level 3]
    /// </summary>
    public static class GlobalMediator
    {
        private static readonly HashSet<IComponent> Components = new ();

        public static IDisposable Connect(IComponent component)
        {
            Components.Add(component);
            
            return new MediatorConnectionToken(component);
        }
        
        public static void Disconnect(IComponent component)
        {
            if (component == null || component.IsDisposed) return;
            
            component.IsDisposed = true;
            Components.Remove(component);
        }

        public static void Message(IComponent source, IComponent target, IPayload payload)
        {
            IComponent first = null;
            
            foreach (var c in Components)
            {
                if (c.Equals(target))
                {
                    first = c;
                    break;
                }
            }
            
            first?.Accept(payload);
        }
        
        public static void BoardCast(IComponent source, IPayload payload, Predicate<IComponent> predicate = null)
        {
            foreach (var component in Components)
            {
                if (component != source && HasConditionMet(component, predicate))
                {
                    component.Accept(payload);
                }
            }
        }
        
        public static bool HasConditionMet(IComponent target, Predicate<IComponent> predicate = null) => predicate is null || predicate(target);
    }
    
}