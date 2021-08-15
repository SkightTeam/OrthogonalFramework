using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Machine.Specifications;

namespace Orthogonal.Persistence.LiteDB.Tests
{
    class when_get_by_id: RepositoryImplSpecs
    {
        private Because of = () =>
        {
            Result = Subject.get("T001").Result;
        };

        private It should_return = () => Result.Should().NotBeNull();
        private It result_id_should_T001 = () => Result.Id.Should().Be("T001");

        public static TestEntity Result { get; set; }
    }
}
