using Newtonsoft.Json;
  
namespace VSS.Productivity3D.Scheduler.Common.Models
{
  /// <summary>
  /// Description to identify a file by its location in TCC.
  /// </summary>
  public class SchedulerFileDescriptor
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
    private SchedulerFileDescriptor()
    {
    }

    /// <summary>
    /// Create instance of SchedulerFileDescriptor
    /// </summary>
    public static SchedulerFileDescriptor CreateFileDescriptor
    (
      string filespaceId,
      string path,
      string fileName
    )
    {
      return new SchedulerFileDescriptor
      {
        filespaceId = filespaceId,
        path = path,
        fileName = fileName
      };
    }

    /// <summary>
    /// Create instance of SchedulerFileDescriptor
    /// </summary>
    public static SchedulerFileDescriptor CreateFileDescriptor
    (
      string filespaceId,
      string customerUid,
      string projectUid,
      string fileName
    )
    {
      return new SchedulerFileDescriptor
      {
        filespaceId = filespaceId,
        path = $"/{customerUid}/{projectUid}",
        fileName = fileName
      };
    }

    public static SchedulerFileDescriptor EmptyFileDescriptor
    {
      get { return emptyDescriptor; }
    }

    private static SchedulerFileDescriptor emptyDescriptor = new SchedulerFileDescriptor
    {
      filespaceId = string.Empty,
      path = string.Empty,
      fileName = string.Empty
    };

  }
}