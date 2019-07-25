using System;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Factories;
using Xunit;

namespace VSS.TRex.Tests.Designs
{
  public class DesignClassFactoryTests
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

      var factory = new DesignClassFactory();
      var siteModelUid = Guid.NewGuid();
      var design = factory.NewInstance(fileName, 1.0, siteModelUid);

      design.Should().NotBeNull();
      design.Should().BeOfType<TTMDesign>();
      design.FileName.Should().Be(fileName);
      design.DataModelID.Should().Be(siteModelUid);
    }

    [Fact]
    public void CreateSVLAlignmentDesign()
    {
      const string fileName = "Bob.svl";

      var factory = new DesignClassFactory();
      var siteModelUid = Guid.NewGuid();
      var design = factory.NewInstance(fileName, 1.0, siteModelUid);

      design.Should().NotBeNull();
      design.Should().BeOfType<SVLAlignmentDesign>();
      design.FileName.Should().Be(fileName);
      design.DataModelID.Should().Be(siteModelUid);
    }

    [Fact]
    public void CreateDesign_Failure()
    {
      const string fileName = "Bob.xxx";

      var factory = new DesignClassFactory();
      var siteModelUid = Guid.NewGuid();
      Action act = () => _ = factory.NewInstance(fileName, 1.0, siteModelUid);
      act.Should().Throw<TRexException>().WithMessage($"Unknown design file type in design class factory for design {fileName}");
    }

    [Fact]
    public void Create_WithComplexExtension()
    {
      const string fileName = "Bob.xxx.svl";

      var factory = new DesignClassFactory();
      var siteModelUid = Guid.NewGuid();
      var design = factory.NewInstance(fileName, 1.0, siteModelUid);

      design.Should().NotBeNull();
      design.Should().BeOfType<SVLAlignmentDesign>();
    }
  }
}
