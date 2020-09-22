using CCSS.WorksOS.Healthz.Services;
using FluentAssertions;
using Xunit;

namespace CCSS.WorksOS.Healthz.UnitTests
{
  public class ServiceResolverTests
  {
    [Fact]
    public void Known_identifiers_Should_be_populated() =>
      ServiceResolver.GetKnownServiceIdentifiers().Should().NotBeNullOrEmpty();

    [Fact]
    public void Known_identifiers_Should_not_contain_duplicates() =>
      ServiceResolver.GetKnownServiceIdentifiers().Should().OnlyHaveUniqueItems();

    [Fact]
    public void Known_identifiers_Should_not_contain_ignored_services()
    {
      var identifiers = ServiceResolver.GetKnownServiceIdentifiers();

      identifiers.Should().NotContain(new[] { "assetmgmt3d-service" });
    }
  }
}
