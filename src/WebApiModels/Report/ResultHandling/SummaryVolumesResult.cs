
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiModels.Report.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary Volumes request
  /// </summary>
  public class SummaryVolumesResult : ContractExecutionResult
    {
        protected SummaryVolumesResult(string message) : base(message)
        {
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private SummaryVolumesResult()
        {
        }
        /// <summary>
        /// Zone boundaries
        /// </summary>
        public BoundingBox3DGrid BoundingExtents { get; private set; }
        /// <summary>
        /// Cut volume in m3
        /// </summary>
        public double Cut { get; private set; }
        /// <summary>
        /// Fill volume in m3
        /// </summary>
        public double Fill { get; private set; }
        /// <summary>
        /// Cut area in m2
        /// </summary>
        public double CutArea { get; private set; }
        /// <summary>
        /// Fill area in m2
        /// </summary>
        public double FillArea { get; private set; }
        /// <summary>
        /// Total coverage area (cut + fill + no change) in m2. 
        /// </summary>
        public double TotalCoverageArea { get; private set; }

        public static SummaryVolumesResult CreateSummaryVolumesResult(BoundingBox3DGrid convertExtents, double cut,
                double fill, double totalCoverageArea, double cutArea, double fillArea)
        {
            return new SummaryVolumesResult()
                   {
                           BoundingExtents = convertExtents,
                           Cut = cut,
                           Fill = fill,
                           TotalCoverageArea = totalCoverageArea,
                           CutArea = cutArea, FillArea = fillArea
                   };
        }

        /// <summary>
        /// Create example instance of SummaryVolumesResult to display in Help documentation.
        /// </summary>
        public static SummaryVolumesResult HelpSample
        {
          get
          {
              return new SummaryVolumesResult()
                     {
                             BoundingExtents = BoundingBox3DGrid.HelpSample,
                             Cut = 13.2,
                             Fill = 11.3,
                             TotalCoverageArea = 132,
                             CutArea = 57.5,
                             FillArea = 62.8
                     };
          }
          
        }
    }
}