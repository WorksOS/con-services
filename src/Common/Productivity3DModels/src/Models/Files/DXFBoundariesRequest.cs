﻿using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.FIlters;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Models.Models.Files
{
  public class DXFBoundariesRequest
  {
    [JsonProperty(PropertyName = "csib", Required = Required.Always)]
    public string CSIB { get; set; }

    [JsonProperty(PropertyName = "fileType", Required = Required.Always)]
    public ImportedFileType FileType { get; set; }

    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    [ValidFilename(256)]
    public string FileName { get; set; }

    [JsonProperty(PropertyName = "fileUnits", Required = Required.Always)]
    public DxfUnitsType FileUnits { get; set; }

    [JsonProperty(PropertyName = "maxBoundaries", Required = Required.Always)]
    public uint MaxBoundaries { get; set; }

    private DXFBoundariesRequest()
    {
    }

    public DXFBoundariesRequest(string csib, ImportedFileType fileType, string fileName, DxfUnitsType fileUnits, uint maxBoundaries)
    {
      CSIB = csib;
      FileType = fileType;
      FileName = fileName;
      FileUnits = fileUnits;
      MaxBoundaries = maxBoundaries;
    }

    public void Validate()
    {
      if (FileType != ImportedFileType.SiteBoundary && FileType != ImportedFileType.Linework)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must support DXF representation"));
      }

      if (FileName == null || string.IsNullOrEmpty(FileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File name must be provided"));
      }

      var extension = Path.GetExtension(FileName);
      if (string.IsNullOrEmpty(extension) ||
          ((string.Compare(extension, ".dxf", StringComparison.OrdinalIgnoreCase) != 0)))
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File name extension incorrect, expected .dxf"));
      }
    }
  }
}
