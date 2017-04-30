using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiModels.FileAccess.Models
{
    public class FileAccessRequest : IValidatable
  {
    /// <summary>
    /// The description of where the file is located in TCC.
    /// </summary>
    [JsonProperty(PropertyName = "file", Required = Required.Always)]
    public FileDescriptor file { get; private set; }

    /// <summary>
    /// The description of where to put the copy of the file.
    /// </summary>
    [JsonProperty(PropertyName = "localPath", Required = Required.Always)]
    public string localPath { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private FileAccessRequest()
    { }

    /// <summary>
    /// Create instance of FileAccessRequest
    /// </summary>
    public static FileAccessRequest CreateFileAccessRequest(
      FileDescriptor file,
      string localPath
      )
    {
      return new FileAccessRequest
      {
        file = file,
        localPath = localPath
      };
    }

    /// <summary>
    /// Example for Help
    /// </summary>
    public static FileAccessRequest HelpSample
    {
      get
      {
        return new FileAccessRequest
        {
          file = FileDescriptor.CreateFileDescriptor("u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01", "/77561/1158", "Large Sites Road - Trimble Road.ttm"),
          localPath = @"D:\ProductionData\DataModels\1158\Temp\Large Sites Road - Trimble Road.ttm"
        };
      }
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      // ...
    }
  }
}
