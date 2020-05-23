using System;
using System.Collections.Generic;

namespace Orthogonal.Persistence.EventStore
{
    public abstract class EventSourcedBase : EventSourced
    {
        private readonly Dictionary<Type, Action<VersionedEvent>> handlers = new Dictionary<Type, Action<VersionedEvent>>();
        private readonly List<VersionedEvent> pendingEvents = new List<VersionedEvent>();

        private int version = -1;

        public string Id { get; protected set; }

        public int Version {
            get { return version; }
            protected set { version = value; }
        }

        public IEnumerable<VersionedEvent> Events {
            get { return pendingEvents; }
        }

        protected void handles<TEvent>(Action<TEvent> handler)
            where TEvent : Event
        {
            this.handlers.Add(typeof(TEvent), @event => handler((TEvent)@event));
        }

        protected void load_from(IEnumerable<VersionedEvent> pastEvents)
        {
            foreach (var e in pastEvents)
            {
                this.handlers[e.GetType()].Invoke(e);
                this.version = e.Version;
            }
        }

        protected void apply(VersionedEvent e)
        {
            e.Version = version + 1;
            handlers[e.GetType()].Invoke(e);
            version = e.Version;
            pendingEvents.Add(e);
        }
    }
}