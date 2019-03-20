using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Gateway.WebApi.ActionServices;
using Xunit;

namespace VSS.TRex.Gateway.Tests.ActionServices
{
  public class CoordinateServiceUtilityTests : IDisposable
  {
    private const string DIMENSIONS_2012_DC_CSIB = "QM0G000ZHC4000000000800BY7SN2W0EYST640036P3P1SV09C1G61CZZKJC976CNB295K7W7G30DA30A1N74ZJH1831E5V0CHJ60W295GMWT3E95154T3A85H5CRK9D94PJM1P9Q6R30E1C1E4Q173W9XDE923XGGHN8JR37B6RESPQ3ZHWW6YV5PFDGCTZYPWDSJEFE1G2THV3VAZVN28ECXY7ZNBYANFEG452TZZ3X2Q1GCYM8EWCRVGKWD5KANKTXA1MV0YWKRBKBAZYVXXJRM70WKCN2X1CX96TVXKFRW92YJBT5ZCFSVM37ZD5HKVFYYYMJVS05KA6TXFY6ZE4H6NQX8J3VAX79TTF82VPSV1KVR8W9V7BM1N3MEY5QHACSFNCK7VWPNY52RXGC1G9BPBS1QWA7ZVM6T2E0WMDY7P6CXJ68RB4CHJCDSVR6000047S29YVT08000";

    public CoordinateServiceUtilityTests()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddTransient<ICoordinateServiceUtility, CoordinateServiceUtility>())
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton<ITPaasProxy, TPaasProxy>())
        .Complete();
    }

    [Fact]
    public void MachineStatusConvertCoords_NoMachines()
    {
      var CSIB = DIMENSIONS_2012_DC_CSIB;
      var machines = new List<MachineStatus>();

      var result = DIContext.Obtain<ICoordinateServiceUtility>().PatchLLH(CSIB, machines);
      result.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
    }

    [Fact(Skip = "Skip until coreX is available")]
    public void MachineStatusConvertCoords_OneMachine()
    {
      var CSIB = DIMENSIONS_2012_DC_CSIB;
      var machines = new List<MachineStatus>() {new MachineStatus
        (Consts.LEGACY_ASSETID, "", false, "", null, null,
          lastKnownLatitude: null, lastKnownLongitude: null,
          lastKnownX: 2313, lastKnownY: 1204)
      };

      var result = DIContext.Obtain<ICoordinateServiceUtility>().PatchLLH(CSIB, machines);
      result.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      machines[0].lastKnownLongitude.Should().Be(-115.05065884125976);
      machines[0].lastKnownLatitude.Should().Be(36.196461918456677);
    }

    [Fact(Skip = "Skip until coreX is available")]
    public void MachineStatusConvertCoords_TwoMachine()
    {
      var CSIB = DIMENSIONS_2012_DC_CSIB;
      var machines = new List<MachineStatus>() {new MachineStatus
        (Consts.LEGACY_ASSETID, "machine1", false, "", null, null,
          lastKnownLatitude: null, lastKnownLongitude: null,
          lastKnownX: 2313, lastKnownY: 1204),
        new MachineStatus
        (Consts.LEGACY_ASSETID, "machine2", false, "", null, null,
          lastKnownLatitude: null, lastKnownLongitude: null,
          lastKnownX: 2314, lastKnownY: 1205)
      };

      var result = DIContext.Obtain<ICoordinateServiceUtility>().PatchLLH(CSIB, machines);
      result.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      machines[0].lastKnownLongitude.Should().Be(-115.05065884125976);
      machines[0].lastKnownLatitude.Should().Be(36.196461918456677);
      machines[1].lastKnownLongitude.Should().Be(-115.05065884125976);
      machines[1].lastKnownLatitude.Should().Be(36.196461918456677);
    }

    public void Dispose()
    {
      DIBuilder.Eject();
    }
  }
}
