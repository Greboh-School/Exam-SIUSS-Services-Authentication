using School.Shared.Tools.Test.Containers;
using Xunit;

namespace School.Exam.SIUSS.Services.Authentication.IntegrationTests.Setup;

[CollectionDefinition("mysql")]
public class ContainerCollection : ICollectionFixture<MySQLContainerFixture>
{
    
}