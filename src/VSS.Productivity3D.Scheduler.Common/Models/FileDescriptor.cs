using Newtonsoft.Json;
  
namespace VSS.Productivity3D.Scheduler.Common.Models
{
  /// <summary>
  /// Description to identify a file by its location in TCC.
  /// </summary>
  public class FileDescriptor
  {
    /// <summary>
    /// The id of the filespace in TCC where the file is located.
    /// </summary>
    [JsonProperty(PropertyName = "filespaceId", Required = Required.Always)]
    public string filespaceId { get; private set; }

    /// <summary>
    /// The full path of the file.
    /// </summary>
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path { get; private set; }

    /// <summary>
    /// The name of the file.
    /// </summary>
    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    public string fileName { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private FileDescriptor()
    {
    }

    /// <summary>
    /// Create instance of FileDescriptor
    /// </summary>
    public static FileDescriptor CreateFileDescriptor
    (
      string filespaceId,
      string path,
      string fileName
    )
    {
      return new FileDescriptor
      {
        filespaceId = filespaceId,
        path = path,
        fileName = fileName
      };
    }

    /// <summary>
    /// Create instance of FileDescriptor
    /// </summary>
    public static FileDescriptor CreateFileDescriptor
    (
      string filespaceId,
      string customerUid,
      string projectUid,
      string fileName
    )
    {
      return new FileDescriptor
      {
        filespaceId = filespaceId,
        path = $"/{customerUid}/{projectUid}",
        fileName = fileName
      };
    }

    public static FileDescriptor EmptyFileDescriptor
    {
      get { return emptyDescriptor; }
    }

    /// <summary>
    /// Create example instance of FileDescriptor to display in Help documentation.
    /// </summary>
    public static FileDescriptor HelpSample
    {
      get
      {
        return new FileDescriptor()
        {
          filespaceId = "u72003136-d859-4be8-86de-c559c841bf10",
          path = "/customerUIDLoc/ProjectUidLoc",
          fileName = "Cycleway.ttm"
        };
      }
    }

    private static FileDescriptor emptyDescriptor = new FileDescriptor
    {
      filespaceId = string.Empty,
      path = string.Empty,
      fileName = string.Empty
    };

  }
}