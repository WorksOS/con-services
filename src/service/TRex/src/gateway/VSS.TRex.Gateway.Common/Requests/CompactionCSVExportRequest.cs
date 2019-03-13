using System;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Requests
{
  /// <summary>
  /// The request representation used to request export data.
  /// </summary>
  public class CompactionCSVExportRequest : CompactionExportRequest
  {
    // coord type can be used to determine the export request type
    public CoordType CoordType { get; private set; }

    public OutputTypes OutputType { get; private set; }

    public UserPreferences UserPreferences { get; private set; }

    // machineNames is for veta export only
    public string[] MachineNames { get; private set; }

    // the following 2 flags are for pass count export only
    public bool RestrictOutputSize { get; private set; }

    public bool RawDataAsDBase { get; private set; }

    private CompactionCSVExportRequest()
    {
    }

    public CompactionCSVExportRequest(
      Guid projectUid,
      FilterResult filter,
      string fileName,
      CoordType coordType,
      OutputTypes coordinateOutputType,
      UserPreferences userPreferences,
      string[] machineNames,
      bool restrictOutputSize,
      bool rawDataAsDBase
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      FileName = fileName;
      CoordType = coordType;
      OutputType = coordinateOutputType;
      MachineNames = machineNames;
      UserPreferences = userPreferences;
      RestrictOutputSize = restrictOutputSize;
      RawDataAsDBase = rawDataAsDBase;
    }
  }
}
