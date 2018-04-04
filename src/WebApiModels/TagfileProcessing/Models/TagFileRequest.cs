using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Common.Filters.Validation;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.TagfileProcessing.Models
{
  /// <summary>
  /// TAG file domain object. Model represents TAG file submitted to Raptor.
  /// The project, identified by ID/UID, to process the TAG file into. These project identifires are optional. 
  /// If not set, Raptor will determine automatically which project the TAG file should be processed into. 
  /// When provided it acts as an override value. 
  /// </summary>
  public class TagFileRequest : /* ProjectID,*/ IValidatable
    {

        /// <summary>
        /// Dummy project ID field to keep things in order until we straighten the TAG files submission endpoint out.
        /// </summary>
        [JsonProperty(PropertyName = "projectId", Required = Required.Default)]
        public long? projectId { get; private set; }


        /// <summary>
        /// The name of the TAG file.
        /// </summary>
        /// <value>Required. Shall contain only ASCII characters. Maximum length is 256 characters.</value>
        [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
        [Required]
        [ValidFilename(256)]
        [MaxLength(256)]
        public string fileName { get; private set; }

        /// <summary>
        /// The content of the TAG file as an array of bytes.
        /// </summary>
        [JsonProperty(PropertyName = "data", Required = Required.Always)]
        [Required]
        public byte[] data { get; private set; }
/*
        /// <summary>
        /// The project to process the TAG file into. This is optional. If not set, Raptor will determine automatically which project the TAG file should be processed into. When provided it acts as an override value.
        /// </summary>
        [JsonProperty(PropertyName = "projectId", Required = Required.Default)]
        [ValidProjectID]  
        public long projectId { get;  set; }
*/
      /* Not implemented
        /// <summary>
        /// The start date restriction of the project to process the TAG file into. If the TAG file is recorded prior to this date it will not be processed into the project.
        /// </summary>
        [JsonProperty(PropertyName = "startDate", Required = Required.Default)] 
        public DateTime startDate { get; private set; }

        /// <summary>
        /// The end date restriction of the project to process the TAG file into. If the TAG file is recorded after this date it will not be processed into the project.
        /// May be null.
        /// </summary>
        [JsonProperty(PropertyName = "endDate", Required = Required.Default)] 
        [DateGreaterThan("startDate")]
        public DateTime endDate { get; private set; }

       */
 
        /// <summary>
        /// The boundary of the project to process the TAG file into. If the location of the data in the TAG file is outside of this boundary it will not be processed into the project.
        /// May be null.
        /// </summary>
        [JsonProperty(PropertyName = "boundary", Required = Required.Default)]
        public WGS84Fence boundary { get; private set; }

        /// <summary>
        /// The machine (asset) ID to process the TAG file as. When not provided the TagProc service will use the project listener to determine the machine/asset ID. When provided it acts as an override value.
        /// May be null.
        /// </summary>
        [JsonProperty(PropertyName = "machineId", Required = Required.Default)]
        public long? machineId { get; private set; }


      [JsonProperty(PropertyName = "tccOrgId", Required = Required.Default)]
      public string tccOrgId { get; private set; }
    /*   /// <summary>
       /// A flag to indicate if the TAG file should also be converted into a CSV file. Not currently available.
       /// </summary>
       [JsonProperty(PropertyName = "convertToCSV", Required = Required.Default)]
       public bool convertToCSV { get; private set; }

       /// <summary>
       /// A flag to indicate if the TAG file should also be converted into a DXF file. Not currently available.
       /// </summary>
       [JsonProperty(PropertyName = "convertToDXF", Required = Required.Default)]
       public bool convertToDXF { get; private set; }*/

    /// <summary>
    /// Private constructor
    /// </summary>
    private TagFileRequest()
        {}

      /// <summary>
        /// Create instance of TAGFile
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="data">metadata</param>
        /// <param name="projectId">project id</param>
        /// <param name="boundary"></param>
        /// <param name="machineId"></param>
        /// <param name="convertToCSV"></param>
        /// <param name="convertToDXF"></param>
        /// <returns></returns>
        public static TagFileRequest CreateTagFile(string fileName,
            byte[] data,
            long projectId,
         //   DateTime startDate,
        //    DateTime endDate,
            WGS84Fence boundary,
            long machineId,
            bool convertToCSV,
            bool convertToDXF,
            string tccOrgId=null)
        {
          return new TagFileRequest
          {
                     fileName = fileName,
                     data = data,
                     projectId = projectId,
                     boundary = boundary,
                     machineId = machineId,
                     /*convertToCSV = convertToCSV,
                     convertToDXF = convertToDXF*/
                     tccOrgId = tccOrgId
                 };
        }
        
        /// <summary>
        /// Validates all properties
        /// </summary>
        public /* override */ void Validate()
        {         
          // base.Validate();
        }

    }
}