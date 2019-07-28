using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL.DXF;
using Xunit;

namespace VSS.TRex.Designs.SVL.Tests
{
  public class DXFOutputRegressionTests
  {
    [Fact]
    public void Creation_DXFFile()
    {
      var dxf = new DXFFile();
      dxf.Should().NotBeNull();
    }

    [Fact]
    public void Creation_ExportToDXF()
    {
      var export = new ExportToDXF();
      export.Should().NotBeNull();
    }

    [Theory]
    [InlineData("CERA.SVL", "CERA.MasterAlignment.DXF")]
    [InlineData("Large Sites Road - Trimble Road.svl", "Large Sites Road - Trimble Road.MasterAlignment.DXF")]
    [InlineData("Topcon Road - Topcon Phil.svl", "Topcon Road - Topcon Phil.MasterAlignment.DXF")]
    [InlineData("Milling - Milling.svl", "Milling - Milling.MasterAlignment.DXF")]
    public void CreateDXFFromMasterAlignment(string fileName, string compareFileName)
    {
      var f = NFFFile.CreateFromFile(Path.Combine("TestData", "Common", fileName));
      var master = f.GuidanceAlignments?.Where(x => x.IsMasterAlignment()).FirstOrDefault();
      master.Should().NotBeNull();

      var export = new ExportToDXF
      {
        AlignmentLabelingInterval = 10,
        Units = DistanceUnitsType.metres
      };

      export.ConstructSVLCenterlineDXFAlignment(master, out var calcResult, out var MS).Should().BeTrue();
      calcResult.Should().Be(DesignProfilerRequestResult.OK);
      MS.Should().NotBeNull();

     // File.WriteAllBytes(Path.GetTempFileName() + fileName + ".MasterAlignment.DXF", MS.ToArray());

      // Compare with known good file
      var goodFile = File.ReadAllBytes(Path.Combine("TestData", "Common", compareFileName));

      MS.ToArray().Should().BeEquivalentTo(goodFile);
    }
  }
}
