using System;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class DesignSubGridRequestArgumentBase : BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public DesignSubGridRequestArgumentBase()
    {
    }

    /// <summary>
    /// Constructor taking the full state of the elevation patch computation operation
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="referenceDesignUID"></param>
    /// <param name="offset"></param>
    protected DesignSubGridRequestArgumentBase(Guid siteModelID,
                                     Guid referenceDesignUID,
                                     double offset) : this()
    {
      ProjectID = siteModelID;
      ReferenceDesign.DesignID = referenceDesignUID;
      ReferenceDesign.Offset = offset;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString() + $" -> SiteModel:{ProjectID}, Design:{ReferenceDesign.DesignID}, Offset:{ReferenceDesign.Offset}";
    }

  }
}
