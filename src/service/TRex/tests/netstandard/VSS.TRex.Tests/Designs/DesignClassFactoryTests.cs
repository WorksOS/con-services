using System;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Factories;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Designs
{
  public class DesignClassFactoryTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Creation()
    {
      var factory = new DesignClassFactory();
      factory.Should().NotBeNull();
    }

    [Fact]
    public void CreateTTMDesign()
    {
      const string fileName = "Bob.ttm";

      var designUid = Guid.NewGuid();
      var factory = new DesignClassFactory();
      var siteModelUid = Guid.NewGuid();
      var design = factory.NewInstance(designUid, fileName, 1.0, siteModelUid);

      design.Should().NotBeNull();
      design.Should().BeOfType<TTMDesign>();
      design.DesignUid.Should().Be(designUid);
      design.FileName.Should().Be(fileName);
      design.ProjectUid.Should().Be(siteModelUid);
    }

    [Fact]
    public void CreateSVLAlignmentDesign()
    {
      const string fileName = "Bob.svl";

      var designUid = Guid.NewGuid();
      var factory = new DesignClassFactory();
      var siteModelUid = Guid.NewGuid();
      var design = factory.NewInstance(designUid, fileName, 1.0, siteModelUid);

      design.Should().NotBeNull();
      design.Should().BeOfType<SVLAlignmentDesign>();
      design.DesignUid.Should().Be(designUid);
      design.FileName.Should().Be(fileName);
      design.ProjectUid.Should().Be(siteModelUid);
    }

    [Fact]
    public void CreateDesign_Failure()
    {
      const string fileName = "Bob.xxx";

      var designUid = Guid.NewGuid();
      var factory = new DesignClassFactory();
      var siteModelUid = Guid.NewGuid();
      Action act = () => _ = factory.NewInstance(designUid, fileName, 1.0, siteModelUid);
      act.Should().Throw<TRexException>().WithMessage($"Unknown design file type in design class factory for design {fileName}");
    }

    [Fact]
    public void Create_WithComplexExtension()
    {
      const string fileName = "Bob.xxx.svl";

      var designUid = Guid.NewGuid();
      var factory = new DesignClassFactory();
      var siteModelUid = Guid.NewGuid();
      var design = factory.NewInstance(designUid, fileName, 1.0, siteModelUid);

      design.Should().NotBeNull();
      design.Should().BeOfType<SVLAlignmentDesign>();
      design.DesignUid.Should().Be(designUid);
    }
  }
}
