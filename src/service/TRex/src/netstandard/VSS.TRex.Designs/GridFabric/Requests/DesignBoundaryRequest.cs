using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Designs.GridFabric.Requests
{
  public class DesignBoundaryRequest : DesignProfilerRequest<DesignBoundaryArgument, DesignBoundaryResponse>
  {
    public override DesignBoundaryResponse Execute(DesignBoundaryArgument arg)
    {
      // Construct the function to be used
      var func = new DesignBoundaryComputeFunc();

      return Compute.Apply(func, arg);
    }

    public static DesignBoundaryResponse Execute(ISiteModel siteModel, DesignOffset referenceDesign)
    {
      var request = new DesignBoundaryRequest();

      return request.Execute(new DesignBoundaryArgument
      {
        ProjectID = siteModel.ID,
        ReferenceDesign = referenceDesign,
        CellSize = siteModel.CellSize
      });
    }

  }
}
