using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.DI;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.TRex.Tests.SiteModels
{
  public class SiteModelMachineTests : IClassFixture<DITagFileFixture>
  {
    [Fact]
    public void GetMachines_NoMachine()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

      var machines = siteModel.Machines.ToList();
      machines.Count.Should().Be(0);
    }

    [Fact]
    public void GetMachines_TwoMachines()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, DeviceType.SNM940, false, Guid.NewGuid());
      IMachine machine2 = siteModel.Machines.CreateNew("Test Machine Source 2", "", MachineType.WheelLoader, DeviceType.SNM941, false, Guid.NewGuid());

      var machines = siteModel.Machines.ToList();
      machines.Count.Should().Be(2);
    }
  }
}
