using System;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Orthogonal.Persistence.AzureTable.Tests
{
    [TestClass]
    public class Given_RepositoryImpl
    {
        private RepositoryImpl<TestEntity> repository;

        [TestInitialize]
        public void SetUp()
        {
            var configuration = Substitute.For<AzureTableConfiguration>();
            configuration.ConnectionString
                .Returns(
                    "your connection string");
            repository = new RepositoryImpl<TestEntity>(configuration);

        }

        [TestMethod]
        public async Task When_save_an_entity()
        {
           var entity = new TestEntity
            {
                Id = $"ID{new Random().Next().ToString()}",
                Name = "My Name"
            };
           await repository.save(entity);
        }
    }

    public class TestEntity : ITableEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PartitionKey { get; set; } = "TestEntityPartition";
        public string RowKey { get => Id; set => Id= value; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}