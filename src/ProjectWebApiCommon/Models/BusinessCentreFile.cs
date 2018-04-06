using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class BusinessCenterFile
  {
    /// <summary>
    /// Gets or sets the filespace id of the file location on TCC.
    /// </summary>
    /// 
    [Required(ErrorMessage = "Required Field")]
    [JsonProperty(PropertyName = "filespaceID", Required = Required.Default)]
    public string FileSpaceId { get; set; }
    /// <summary>
    /// Gets or sets the path to the file location on TCC.
    /// </summary>
    [Required(ErrorMessage = "Required Field")]
    [JsonProperty(PropertyName = "path", Required = Required.Default)]
    public string Path { get; set; }
    /// <summary>
    /// Gets or sets the name of the file on TCC.
    /// </summary>
    //[Required(ErrorMessage = "Required Field")]
    //[StringLength(100, ErrorMessage = "name must be less than 100 characters", MinimumLength = 1)]
    // todo validation
    [JsonProperty(PropertyName = "name", Required = Required.Default)]
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets the UTC date/time the file was created on TCC.
    /// </summary>
    [Required(ErrorMessage = "Required Field")]
    [JsonProperty(PropertyName = "createdUTC", Required = Required.Default)]
    public DateTime CreatedUtc { get; set; }

    /*     
    public static BusinessCenterFile HelpSample
    {
      get
      {
        return new BusinessCenterFile()
        {
          filespaceID = "u710e3466-1d47-45e3-87b8-81d1127ed4ed",
          path = "/BC Data/Sites/Chch Test Site",
          name = "CTCTSITECAL.dc",
          createdUTC = DateTime.UtcNow.AddDays(-0.5)
        };
      }
    }
    */
  }

}