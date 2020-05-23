using System;
using System.Collections.Generic;
using System.Text;

namespace Orthogonal.Persistence.EventStore
{
    public interface ConnectionConfiguration
    {
        string Host { get; }
        int Port { get; }
        string User { get; }
        string Password { get; }
    }
}
