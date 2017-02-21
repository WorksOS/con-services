using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling
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


      /// <summary>
      /// Create example instance of ProjectExtentsResult to display in Help documentation.
      /// </summary>
      public static ProjectExtentsResult HelpSample
      {
        get
        {
          return new ProjectExtentsResult()
          {
            ProjectExtents = BoundingBox3DGrid.HelpSample
          };
        }
      }









  }
}