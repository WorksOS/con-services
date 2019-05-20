using System;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs
{
  /// <summary>
  /// Wraps a loaded design with an offset
  /// </summary>
  public class DesignWrapper : IDesignWrapper
  {
    private DesignOffset DesignOffset { get; set; }
    public IDesign Design { get; set; }

    public Guid DesignID => DesignOffset?.DesignID ?? Guid.Empty;
    public double Offset => DesignOffset?.Offset ?? 0;

    public DesignWrapper() { }

    public DesignWrapper(DesignOffset designOffset, IDesign design)
    {
      DesignOffset = designOffset;
      Design = design;
    }
  }
}
