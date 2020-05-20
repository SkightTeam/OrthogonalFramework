using Machine.Fakes;
using Machine.Specifications;
using NSubstitute;

namespace Orthogonal.Persistence.AzureBlob.Tests
{
    public class RepositoryImplSpec : WithSubject<RepositoryImpl<TestEntity>>
    {
        private Establish context = () => The<BlobClientConfiguration>().ConnectionString.Returns(
            @"DefaultEndpointsProtocol=https;AccountName=ersdb;AccountKey=O/algkCQluzAOSz0loY5i+lONZ+aiMlCVl9OHscCczEB7XconREZYuxB9EcriEExGnjP2hbm3U0/StqhY/w8fA==;EndpointSuffix=core.windows.net");
    }
}