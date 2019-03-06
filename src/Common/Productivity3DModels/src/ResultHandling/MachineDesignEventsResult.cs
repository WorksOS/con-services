using System;
using System.Collections.Generic;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class MachineDesignEventsResult : ContractExecutionResult
  {
    public List<MachineDesignEvent> MachineDesignEvents;

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private MachineDesignEventsResult()
    {
    }

    public MachineDesignEventsResult(List<MachineDesignEvent> machineDesignEvents)
    {
      MachineDesignEvents = machineDesignEvents;
    }
  }

  public class MachineDesignEvent
  {
    private Guid DesignUid;
    private string DesignName;
    private DateTime StartUtc;
    private DateTime EndUtc;
    private Guid MachineUid;

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private MachineDesignEvent()
    {
    }

    public MachineDesignEvent(Guid designUid, string designName, DateTime startUtc, DateTime endUtc, Guid machineUid)
    {
      DesignUid = designUid;
      DesignName = designName;
      StartUtc = startUtc;
      EndUtc = endUtc;
      MachineUid = machineUid;
    }
  }

}
