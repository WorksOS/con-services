using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.MasterData.Models.FIlters;
using VSS.Productivity3D.Interfaces;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Coord.Models
{
  /// <summary>
  /// Coordinate system (CS) definition file domain object. Model represents a coordinate system definition.
  /// </summary>
  public class CoordinateSystemFile : ProjectID, IIsProjectIDApplicable
  {
        public const int MAX_FILE_NAME_LENGTH = 256;

        /// <summary>
        /// The content of the CS definition file as an array of bytes.
        /// </summary>
        [JsonProperty(PropertyName = "csFileContent", Required = Required.Always)]
        [Required]
        public byte[] csFileContent { get; private set; }

        /// <summary>
        /// The name of the CS definition file.
        /// </summary>
        /// 
        [JsonProperty(PropertyName = "csFileName", Required = Required.Always)]
        [ValidFilename(MAX_FILE_NAME_LENGTH)]
        [MaxLength(MAX_FILE_NAME_LENGTH)]
        [Required]
        public string csFileName { get; private set; }
    
        /// <summary>
        /// Private constructor.
        /// </summary>
        /// 
        private CoordinateSystemFile()
        {
            // ...
        }

        /// <summary>
        /// Creates an instance of the CoordinateSystemFile class.
        /// </summary>
        /// <param name="projectId">The project to process the file into.</param>
        /// <param name="csFileContent">The content of the file.</param>
        /// <param name="csFileName">The file's name.</param>
        /// <returns>An instance of the CoordinateSystemFile class.</returns>
        public static CoordinateSystemFile CreateCoordinateSystemFile(long projectId, byte[] csFileContent, string csFileName)
        {
          var tempCS = new CoordinateSystemFile
          {
            ProjectId = projectId,
            csFileName = csFileName,
            csFileContent = csFileContent
          };

          return tempCS;
        }


        /// <summary>
        /// Validation method.
        /// </summary>
        public override void Validate()
        {
          base.Validate();
            // Validation rules might be placed in here...
            // throw new NotImplementedException();
        }

    public bool HasProjectID()
    {
      return true;
    }
  }
}
