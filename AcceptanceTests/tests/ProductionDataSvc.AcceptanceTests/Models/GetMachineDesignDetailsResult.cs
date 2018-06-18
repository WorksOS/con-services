using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class GetMachineDesignDetailsResult : RequestResult, IEquatable<GetMachineDesignDetailsResult>
  {
    #region Members
    public List<MachineDesignDetails> machineDesignDetails { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor: success result by default
    /// </summary>
    public GetMachineDesignDetailsResult()
      : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(GetMachineDesignDetailsResult other)
    {
      if (other == null)
        return false;

      if (this.machineDesignDetails.Count != other.machineDesignDetails.Count)
        return false;

      for (int i = 0; i < this.machineDesignDetails.Count; ++i)
      {
        if (!this.machineDesignDetails[i].Equals(other.machineDesignDetails[i]))
          return false;
      }

      return this.Code == other.Code && this.Message == other.Message;
    }

    public static bool operator ==(GetMachineDesignDetailsResult a, GetMachineDesignDetailsResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(GetMachineDesignDetailsResult a, GetMachineDesignDetailsResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is GetMachineDesignDetailsResult && this == (GetMachineDesignDetailsResult)obj;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
    #endregion

    #region ToString override
    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
    #endregion
  }
}
