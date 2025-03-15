using FluentAssertions;
using NetArchTest.Rules;

namespace EShop.Identity.Tests.Architecture;

public class ArchitectureTests
{
    private const string DomainNamespace = "EShop.Identity.Domain";
    private const string ApplicationNamespace = "EShop.Identity.Application";
    private const string PersistenceNamespace = "EShop.Identity.Persistence";
    private const string InfrastructureNamespace = "EShop.Identity.Infrastructure";
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

    [Fact]
    public void Application_Should_Not_HaveDependencyOtherProjects()
    {
        var application = Application.AssemblyReference.Assembly;
        var otherProjects = new[]
        {
            InfrastructureNamespace,
            PersistenceNamespace,
            PresentationNamespace,
            ApiNamespace
        };

        var result = Types
            .InAssembly(application)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Persistence_Should_Not_HaveDependencyOnOtherProjects()
    {
        var persistence = Persistence.AssemblyReference.Assembly;
        var otherProjects = new[]
        {
            PresentationNamespace,
            ApiNamespace
        };

        var result = Types
            .InAssembly(persistence)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_Should_Not_HaveDependencyOnOtherProjects()
    {
        var infrastructure = Infrastructure.AssemblyReference.Assembly;
        var otherProjects = new[]
        {
            PresentationNamespace,
            ApiNamespace
        };

        var result = Types
            .InAssembly(infrastructure)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Presentation_Should_Not_HaveDependencyOnOtherProjects()
    {
        var presentation = Presentation.AssemblyReference.Assembly;
        var otherProjects = new[]
        {
            InfrastructureNamespace,
            PersistenceNamespace,
            DomainNamespace,
        };

        var result = Types
            .InAssembly(presentation)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}