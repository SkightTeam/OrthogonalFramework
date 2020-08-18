using System;
using System.Collections.Generic;
using System.Text;

namespace Orthogonal.Persistence.EventStore
{
    public interface Configuration
    {
        Server Server { get; }
        Credential Admin { get; }
        Credential Operator { get; }
      
    }

    public interface Server
    {
        string Host { get;  }
        int TcpPort { get; }
        int  HttpPort { get; }
    }

    public interface Credential
    {
        public string Name { get;  }
        public string Password { get; }
    }

}
