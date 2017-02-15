using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.ResultHandling
{
  /// <summary>
  /// The result representation of a get project boundaries request.
  /// </summary>
  public class GetProjectBoundariesAtDateResult : ContractExecutionResult // , IHelpSample
  {
    /// <summary>
    /// The result of the request. True for success and false for failure.
    /// </summary>
    public bool result { get; private set; }

    /// <summary>
    /// The boundaries of the projects. Empty if none.
    /// </summary>
    public ProjectBoundaryPackage[] projectBoundaries { get; private set; }

    ///// <summary>
    ///// Private constructor
    ///// </summary>
    //private GetProjectBoundariesAtDateResult()
    //{ }

    /// <summary>
    /// Create instance of GetProjectBoundariesAtDateResult
    /// </summary>
    public static GetProjectBoundariesAtDateResult CreateGetProjectBoundariesAtDateResult(bool result, ProjectBoundaryPackage[] projectBoundaries)
    {
      return new GetProjectBoundariesAtDateResult
      {
        result = result,
        projectBoundaries = projectBoundaries
      };
    }

    /// <summary>
    /// Example for Help
    /// </summary>
    public static GetProjectBoundariesAtDateResult HelpSample
    {
      get
      {
        return CreateGetProjectBoundariesAtDateResult(true, new ProjectBoundaryPackage[] { new ProjectBoundaryPackage { Boundary = GetProjectBoundaryAtDateResult.HelpSample.projectBoundary, ProjectID = 1423 } });
      }
    }
  }
}