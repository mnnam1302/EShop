using FluentAssertions;
using NetArchTest.Rules;

namespace EShop.Identity.Tests.Architecture;

public class ArchitectureTests
{
    private const string DomainNamespace = "EShop.Identity.Domain";
    private const string ApplicationNamespace = "EShop.Identity.Application";
    private const string InfrastructureNamespace = "EShop.Identity.Infrastructure";
    private const string PersistenceNamespace = "EShop.Identity.Persistence";
    private const string PresentationNamespace = "EShop.Identity.Presentation";
    private const string ApiNamespace = "EShop.Identity.Api";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOtherProjects()
    {
        var domain = Domain.AssemblyReference.Assembly;
        var otherProjects = new[]
        {
            ApplicationNamespace,
            InfrastructureNamespace,
            PersistenceNamespace,
            PresentationNamespace,
            ApiNamespace
        };

        var result = Types
            .InAssembly(domain)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}