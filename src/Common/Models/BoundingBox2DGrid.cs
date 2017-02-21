
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.Common.Models
{
    /// <summary>
    /// Defines a bounding box representing a 2D grid coorindate area
    /// </summary>
  public class BoundingBox2DGrid : IValidatable
    {
        /// <summary>
        /// The bottom left corner of the bounding box, expressed in meters
        /// </summary>
        [JsonProperty(PropertyName = "bottomLeftX", Required = Required.Always)]
        public double bottomLeftX { get; set; }
        /// <summary>
        /// The bottom left corner of the bounding box, expressed in meters
        /// </summary>
        [JsonProperty(PropertyName = "bottomleftY", Required = Required.Always)]
        public double bottomleftY { get; set; }
        /// <summary>
        /// The top right corner of the bounding box, expressed in meters
        /// </summary>
        [JsonProperty(PropertyName = "topRightX", Required = Required.Always)]
        public double topRightX { get; set; }
        /// <summary>
        /// The top right corner of the bounding box, expressed in meters
        /// </summary>
        [JsonProperty(PropertyName = "topRightY", Required = Required.Always)]
        public double topRightY { get; set; }

     /// <summary>
      /// Private constructor
      /// </summary>
        private BoundingBox2DGrid()
      {}

      /// <summary>
        /// Create instance of BoundingBox2DGrid
      /// </summary>
        public static BoundingBox2DGrid CreateBoundingBox2DGrid(
        double botLeftX,
        double botLeftY,
        double topRightX,
        double topRightY
        )
      {
        return new BoundingBox2DGrid
               {
                   bottomLeftX = botLeftX,
                   bottomleftY = botLeftY,
                   topRightX = topRightX,
                   topRightY = topRightY
               };
      }

      /// <summary>
        /// Create example instance of BoundingBox2DGrid to display in Help documentation.
      /// </summary>
        public static BoundingBox2DGrid HelpSample
      {
        get
        {
          return new BoundingBox2DGrid()
          {
            bottomLeftX = 380646.982394,
            bottomleftY = 812634.205106,
            topRightX = 380712.19834,
            topRightY = 812788.92875
          };
        }
      }


      /// <summary>
      /// Validates all properties
      /// </summary>
      public void Validate()
      {
        if (bottomLeftX > topRightX || bottomleftY > topRightY)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Invalid bounding box: corners are not bottom left and top right."));               
        }
      }
    }
 }