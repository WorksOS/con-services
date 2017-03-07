using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;

namespace VSS.TagFileAuth.Service.WebApiModels.ResultHandling
{
  /// <summary>
  /// The result representation of a get project boundary request.
  /// </summary>
  public class GetProjectBoundaryAtDateResult : ContractExecutionResult 
  {
    /// <summary>
    /// The result of the request. True for success and false for failure.
    /// </summary>
    public bool result { get; private set; }

    /// <summary>
    /// The boundary of the project. Empty if none.
    /// </summary>
    public TWGS84FenceContainer projectBoundary { get; private set; }

    // acceptance tests cannot serialize with a private const.
    //private GetProjectBoundaryAtDateResult()
    //{ }

    /// <summary>
    /// Create instance of GetProjectBoundaryAtDateResult
    /// </summary>
    public static GetProjectBoundaryAtDateResult CreateGetProjectBoundaryAtDateResult(bool result, TWGS84FenceContainer projectBoundary)
    {
      return new GetProjectBoundaryAtDateResult
      {
        result = result,
        projectBoundary = projectBoundary
      };
    }

    /// <summary>
    /// Example for Help
    /// </summary>
    public static GetProjectBoundaryAtDateResult HelpSample
    {
      get
      {
        TWGS84FenceContainer fenceContainer = new TWGS84FenceContainer();
        fenceContainer.FencePoints = new TWGS84Point[]
        {
          new TWGS84Point{Lat=0.631986074660308, Lon=-2.00757760231466},
          new TWGS84Point{Lat=0.631907507374149, Lon=-2.00758733949739},
          new TWGS84Point{Lat=0.631904485465203, Lon=-2.00744352879854},
          new TWGS84Point{Lat=0.631987283352491, Lon=-2.00743753668608}
        };

        return CreateGetProjectBoundaryAtDateResult(true, fenceContainer);
      }
    }
  }
}