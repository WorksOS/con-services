using System;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignWrapper
  {
    /*
    /// <summary>
    /// Design identifier and offset if it's a reference surface
    /// </summary>
    DesignOffset DesignOffset { get; set; }
    */
    IDesign Design { get; set; }

    Guid DesignID { get; }
    double Offset { get; }
  }
}
