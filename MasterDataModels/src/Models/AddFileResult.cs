using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Models.Models
{
  public class AddFileResult : ContractExecutionResult
  {
    public AddFileResult(int code, string message)
      : base(code, message)
    { }

    /// <summary>
    /// The minimum zoom level that DXF tiles have been generated for.
    /// </summary>
    public int MinZoomLevel;
    /// <summary>
    /// The maximum zoom level that DXF tiles have been generated for.
    /// </summary>
    public int MaxZoomLevel;

    /// <summary>
    /// The unique ID of the file
    /// </summary>
    public Guid FileUid;

    /// <summary>
    /// The details of the file
    /// </summary>
    public FileDescriptor FileDescriptor;

    /// <summary>
    /// The email address of the user who added the file
    /// </summary>
    public string UserEmailAddress;
  }
}
