using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using EventStore.ClientAPI;
using EventStore.ClientAPI.PersistentSubscriptions;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;

namespace Orthogonal.Persistence.EventStore
{
    public class Manager
    {
        public Manager(Configuration configuration)
        {
            this.Configuration = configuration;
            var settings =
                ConnectionSettings.Create()
                    .KeepReconnecting()
                    .KeepRetrying();
            var httpEndPoint = new DnsEndPoint(configuration.Server.Host,configuration.Server.HttpPort);
            Admin =new UserCredentials(Configuration.Admin.Name,configuration.Admin.Password);
            Operator =new UserCredentials(Configuration.Operator.Name,configuration.Operator.Password);

            Connection = EventStoreConnection.Create(
                settings
               ,
                new UriBuilder(
                    "tcp",
                    configuration.Server.Host, 
                    configuration.Server.TcpPort)
                        .Uri
            );
            ProjectionsManager = new ProjectionsManager(
                Connection.Settings.Log,
                httpEndPoint, 
                Connection.Settings.OperationTimeout
                );
            PersistentSubscriptionsManager = new PersistentSubscriptionsManager(
                Connection.Settings.Log,
                httpEndPoint,
                Connection.Settings.OperationTimeout
                );
         
        }


        public Configuration Configuration { get; }
        public IEventStoreConnection Connection { get; }
        public ProjectionsManager ProjectionsManager { get; }
        public PersistentSubscriptionsManager PersistentSubscriptionsManager { get; }
        public UserCredentials Admin { get; }
        public UserCredentials Operator { get; }
    }
}
