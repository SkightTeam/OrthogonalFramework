using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using FluentAssertions;
using Machine.Fakes;
using Machine.Specifications;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Orthogonal.CQRS;

namespace Orthogonal.Persistence.EventStore.Tests
{
    public  class RepositoryImplSpecs : WithSubject<RepositoryImpl<TestEntity>>
    {
        private Establish context = () =>
        {
            var configuration = An<Configuration>();
            configuration.Server.Host.Returns("127.0.0.1");
            configuration.Server.TcpPort.Returns(1113);
            configuration.Server.HttpPort.Returns(2113);
            configuration.Admin.Name.Returns("admin");
            configuration.Admin.Password.Returns("changeit");
            configuration.Operator.Name.Returns("ops");
            configuration.Operator.Password.Returns("changeit");
            manager =new Manager(configuration);
            
            Configure(r=>r.For<Manager>().Use(manager));

          
        };

        protected static Manager manager;
    }

    class When_save_entity : RepositoryImplSpecs
    {
        private Because of = () =>
        {
            Task.Run(async () =>
            {
                
                await manager.Connection.ConnectAsync();
                await Task.Delay(1000);
                entity = new TestEntity(new Random().Next(10000).ToString());
                await Subject.save(entity);
                entity.set(1.2M);
                await Subject.save(entity);
                savedEntity= await Subject.get(entity.Id);
            }).Wait();
        };

        private It should_save_name = () => savedEntity.Name.Should().Be(entity.Name);
        private It should_save_value = () => savedEntity.Value.Should().Be(1.2M);

        private static TestEntity entity;
        private static TestEntity savedEntity;
    }

    class When_query : RepositoryImplSpecs
    {
        private Establish context = () =>
        {
            readModalRepository = new RepositoryImpl<ReadModal>(manager, An<IMemoryCache>());
           
          
        };

        private Because of = () =>
        {
            Task.Run(async () =>
            {
                await manager.Connection.ConnectAsync();
                await Task.Delay(1000);
                entity1 = new TestEntity("No"+DateTime.Now.Ticks);
                await Subject.save(entity1);
                entity2 = new TestEntity("No2"  +DateTime.Now.Ticks);
                await Subject.save(entity2);
            }).Wait();
            result = readModalRepository.search(new ReadModalQuery()).ToListAsync().Result;
        };

        private It should_get_result = () => result[0].EntityNames.Should().Contain(entity1.Name, entity2.Name);
        private static List<ReadModal> result;
        private static  RepositoryImpl<ReadModal> readModalRepository;
        private static TestEntity entity1;
        private static TestEntity entity2;

        class ReadModal
        {
            public List<string> EntityNames { get; set; } = new List<string>();
        }

        class ReadModalQuery : QueryByEvents<ReadModal>
        {
            public async IAsyncEnumerable<ReadModal> apply(IAsyncEnumerable<VersionedEvent> events)
            {
                var result = new ReadModal();
                await foreach (var evt in events)
                {
                    if(evt is EntityCreated created){
                        result.EntityNames.Add(created.Name);
                    }
                }

                yield return result;
            }
        }
    }

}
