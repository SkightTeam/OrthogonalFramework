using System;

namespace Orthogonal.Persistence.LiteDB.Tests
{
    public class TestEntityWithGuid
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }
}