﻿using System;
using VSS.TRex.Designs.Models;
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
    protected DesignSubGridRequestArgumentBase(Guid siteModelId, DesignOffset referenceDesign) : this()
    {
      ProjectID = siteModelId;
      ReferenceDesign = referenceDesign;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString() + $" -> SiteModel:{ProjectID}, Design:{ReferenceDesign?.DesignID}, Offset:{ReferenceDesign?.Offset}";
    }
  }
}
