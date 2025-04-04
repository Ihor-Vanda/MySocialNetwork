using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Tests.IntegrationTests
{
    [CollectionDefinition("TestsCollection")]
    public class TestsCollection : ICollectionFixture<ContainerFixture> { }
}