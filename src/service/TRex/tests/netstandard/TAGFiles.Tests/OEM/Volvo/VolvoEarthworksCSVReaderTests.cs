using System.IO;
using FluentAssertions;
using VSS.TRex.Events;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.TAGFiles.Classes.OEM.Volvo;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests.OEM.Volvo
{
  public class VolvoEarthworksCSVReaderTests : IClassFixture<DITagFileFixture>
  {
    private const string FILENAME = @"C:\Temp\VolvoEarthworksCSVFiles\lift1_Lag 1_1_utm27W_2020-03-11 14-19-07_S135B556186.csv";

    [Fact]
    public void Creation()
    {
      var file = new VolvoEarthworksCSVReader(null);
      file.Should().NotBeNull();
    }

    [Fact]
    public void LoadFile()
    {
      using var stream = new FileStream(FILENAME, FileMode.Open, FileAccess.Read);
      var file = new VolvoEarthworksCSVReader(stream);

      var siteModel = new SiteModel(StorageMutability.Immutable);
      var machine = new Machine();
      var siteModelGridAggregator = new ServerSubGridTree(siteModel.ID, StorageMutability.Mutable);
      var machineTargetValueChangesAggregator = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

      var processor = new TAGProcessor(siteModel, machine, siteModelGridAggregator, machineTargetValueChangesAggregator);

      file.Read(null, processor);

      processor.ProcessedEpochCount.Should().Be(19325);
      siteModelGridAggregator.CountLeafSubGridsInMemory().Should().Be(11);
    }
  }
}
