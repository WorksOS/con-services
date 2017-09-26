using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// ProjectExtentsResult
  /// </summary>
  public class ProjectExtentsResult : ContractExecutionResult
  {
      /// <summary>
      /// BoundingBox3DGrid
      /// </summary>
      public BoundingBox3DGrid ProjectExtents { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
      private ProjectExtentsResult()
        {}


      /// <summary>
      /// ProjectExtentsResult create instance
      /// </summary>
      /// <param name="convertedExtents"></param>
      /// <returns></returns>
      public static ProjectExtentsResult CreateProjectExtentsResult(BoundingBox3DGrid convertedExtents)
      {
        return new ProjectExtentsResult
        {
          ProjectExtents = convertedExtents
        };
      }
  }
}