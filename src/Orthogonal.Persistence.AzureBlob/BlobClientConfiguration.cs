using System;
using System.Collections.Generic;
using System.Text;

namespace Orthogonal.Persistence.AzureBlob
{
    public interface BlobClientConfiguration
    {
        string ConnectionString { get; }
        Func<Type,string> NameConvention { get; }
    }
}
