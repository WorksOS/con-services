using System;
using System.Globalization;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.FIlters;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.Models
{
  public class HydroRequest
  {
    private const string DEFAULT_RESULTANT_FILENAME = "Hydro.zip";

    /// <summary>A project unique identifier.</summary>
    [JsonProperty(PropertyName = "ProjectUID", Required = Required.Default)]
    public Guid ProjectUid { get; set; }

    /// <summary>Filter may contain either: 1 DesignBoundary or GeofenceBoundary (else project boundary is used)</summary>
    [JsonProperty(PropertyName = "FilterUID", Required = Required.Default)]
    public Guid? FilterUid { get; set; }

    /// <summary>Name of the resultant zipped file/s to be returned.</summary>
    [JsonProperty(PropertyName = "FileName", Required = Required.Default)]
    [ValidFilename(256)]
    public string FileName { get; set; } = DEFAULT_RESULTANT_FILENAME;

    /// <summary>Options to analyse design to produce images</summary>

    [JsonProperty(PropertyName = "Options", Required = Required.Default)]
    public HydroOptions Options { get; set; }


    public HydroRequest()
    {
      Initialize();
    }

    private void Initialize()
    {
      ProjectUid = Guid.Empty;
      FilterUid = null;
      FileName = DEFAULT_RESULTANT_FILENAME;
      Options = new HydroOptions();
    }

    public HydroRequest(Guid projectUid, Guid? filterUid, 
      HydroOptions options,
      string fileName = DEFAULT_RESULTANT_FILENAME)
    {
      Initialize();
      ProjectUid = projectUid;
      FilterUid = filterUid;
      FileName = fileName;
      Options = options;
    }

    public void Validate()
    {
      if (ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2001, "Invalid ProjectUid."));
      }

      if (FilterUid != null && FilterUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2002, "Invalid FilterUid."));
      }

      if (string.IsNullOrEmpty(FileName) || !Path.HasExtension(FileName) || string.Compare(Path.GetExtension(FileName), ".zip", true, CultureInfo.InvariantCulture) != 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2003, "Must have a valid resultant zip file name."));
      }

      Options.Validate();
    }
  }
}

