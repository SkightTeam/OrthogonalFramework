using System;
using System.Collections.Generic;
using Orthogonal.CQRS;

namespace Orthogonal.Persistence.EventStore.Tests
{
    public class TestEntity : EventSourcedBase, MementoOriginator
    {
        public TestEntity(string name) : this()
        {
            apply(new EntityCreated
            {
                Name = name
            });
        }

        public string Name { get; set; }
        public decimal Value { get; set; }

        public void set(decimal value)
        {
            apply(new EntityValueAssigned
            {
                Value = value
            });
        }

        #region event sourcing
        public TestEntity(IAsyncEnumerable<VersionedEvent> history) : this()
        {
            load_from(history);
        }

        public TestEntity(Memento memento, IAsyncEnumerable<VersionedEvent> history) : this()
        {
            var state = (TestEntityMemento)memento;
            Id = state.Id;
            Name = state.Name;
            Value = state.Value;
            Version = state.Version;
            load_from(history);
        }

        protected TestEntity()
        {
            handles<EntityCreated>(OnCreated);
            handles<EntityValueAssigned>(OnValueAssigned);
        }

        private void OnCreated(EntityCreated e)
        {
            Id = e.Name;
            Name = e.Name;
        }

        private void OnValueAssigned(EntityValueAssigned e)
        {
            Value = e.Value;
        }
        public Memento save_to_memento()
        {
            return new TestEntityMemento
            {
                Id = Id,
                Version = Version,
                Name = Name,
                Value = Value
            };
        }

        class TestEntityMemento : Memento
        {
            public string Id { get; set; }
            public string  Name { get; set; }
            public decimal  Value { get; set; }
            public int Version { get; set; }
        }
        #endregion
    }
}