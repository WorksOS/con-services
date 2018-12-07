using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// TAG file domain object. Model represents TAG file submitted to Raptor.
  /// </summary>
  public class DxfFileRequest : ProjectID
  {
    /// <summary>
    /// The DXF files submitted for processing.
    /// </summary>
 //   public DxfFile[] FileArray { get; set; }

    public IList<IFormFile> Files { get;set; }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

     // foreach (var file in Files)
      //{
      //  if (file.Data == null || !file.Data.Any())
      //  {
      //    throw new ServiceException(HttpStatusCode.BadRequest,
      //            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
      //                    "File data cannot be null"));
      //  }
      //}
    }
  }

  //public class DxfFile
  //{
  //  /// <summary>
  //  /// The content of the TAG file as an array of bytes.
  //  /// </summary>
  //  [JsonProperty(Required = Required.Always)]
  //  public IList<IFormFile> Files { get; set; }
  //}
}
