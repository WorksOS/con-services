using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.FIlters;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Validation;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.Common.Requests
{
  public class DesignRequest
  {
    [JsonProperty(PropertyName = "projectUid", Required = Required.Always)]
    [ValidProjectUID]
    public Guid ProjectUid { get; set; }

    [JsonProperty(PropertyName = "fileType", Required = Required.Always)]
    public ImportedFileType FileType { get; set; }

    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    [ValidFilename(256)]
    public string FileName { get; set; }

    [JsonProperty(PropertyName = "designUid", Required = Required.Always)]
    public Guid DesignUid { get; set; }

    [JsonProperty(PropertyName = "surveyedUtc", Required = Required.Default)]
    public DateTime? SurveyedUtc { get; set; }


    private DesignRequest()
    {
    }

    public DesignRequest(Guid projectUid, ImportedFileType fileType, string fileName, Guid designUid, DateTime? surveyedUtc)
    {
      ProjectUid = projectUid;
      FileType = fileType;
      FileName = fileName;
      DesignUid = designUid;
      SurveyedUtc = surveyedUtc;
    }

    public void Validate()
    {
      if (!Guid.TryParseExact(ProjectUid.ToString(), "D", out Guid _) || ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "ProjectUid must be provided"));
      }

      if (FileType != ImportedFileType.DesignSurface || FileType != ImportedFileType.SurveyedSurface )
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must be DesignSurface or SurveyedSurface"));
      }

      if (FileName == null || string.IsNullOrEmpty(FileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File name must be provided"));
      }

      if (Path.GetExtension(FileName) != ".ttm")
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File name extension must be ttm"));
      }

      if (!Guid.TryParseExact(DesignUid.ToString(), "D", out Guid _) || DesignUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "DesignUid must be provided"));
      }

      if (FileType == ImportedFileType.SurveyedSurface && SurveyedUtc == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "SurveyedUtc must be provided for a SurveyedSurface file type"));
      }
    }
  }
}
