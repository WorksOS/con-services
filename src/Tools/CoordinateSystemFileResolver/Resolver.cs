using System;
using System.IO;
using System.Text;
using CoordinateSystemFileResolver.Interfaces;
using CoordinateSystemFileResolver.Utils;
using Microsoft.Extensions.Logging;

namespace CoordinateSystemFileResolver
{
  public class Resolver : IResolver
  {
    private readonly ILogger Log;
    private readonly ICSIBAgent _csibAgent;

    private Guid ProjectUid;
    private Guid CustomerUid;
    private string csib;

    private readonly string TemporaryFolder;

    public Resolver(ILoggerFactory logger, IEnvironmentHelper environmentHelper, ICSIBAgent csibAgent)
    {
      Log = logger.CreateLogger<Resolver>();
      _csibAgent = csibAgent;

      TemporaryFolder = Path.Combine(environmentHelper.GetVariable("OUTPUT_FOLDER", 2), "CoordinateSystemFileResolver");
    }

    public IResolver ResolveCSIB(Guid projectUid, Guid customerUid)
    {
      ProjectUid = projectUid;
      CustomerUid = customerUid;

      // Get the CSIB for this project from Raptor, not the Project database, it may be missing or 'invalid'.
      var csibResponse = _csibAgent.GetCSIBForProject(ProjectUid, CustomerUid);
      csib = csibResponse.CSIB;

      return this;
    }

    public void GetCoordSysInfoFromCSIB64()
    {
      var coordSysInfo = _csibAgent.GetCoordSysInfoFromCSIB64(ProjectUid, csib);

      if (coordSysInfo == null)
      {
        Log.LogError($"Failed to retrieve coordinate system file info for Project {ProjectUid}");
        return;
      }

      var dcFileContent = _csibAgent.GetCalibrationFileForCoordSysId(ProjectUid, coordSysInfo["coordinateSystem"]["id"].ToString());

      var coordSystemFileContent = Encoding.UTF8.GetBytes(dcFileContent);

      if (coordSystemFileContent != null && coordSystemFileContent.Length > 0)
      {
        SaveDCFileToDisk(ProjectUid, coordSystemFileContent);
      }
      else
      {
        Log.LogError($"Failed to save coordiante system info to file. Coordinate System service response: {dcFileContent}");
      }
    }

    /// <summary>
    /// Saves the DC file content to disk; for testing purposes only so we can eyeball the content.
    /// </summary>
    private void SaveDCFileToDisk(Guid projectUid, byte[] dcFileContent)
    {
      Log.LogDebug($"Writing coordinate system file for project {projectUid}");

      var dcFilePath = Path.Combine(TemporaryFolder);

      if (!Directory.Exists(dcFilePath))
      {
        Directory.CreateDirectory(dcFilePath);
      }

      var tempFileName = Path.Combine(dcFilePath, $"{projectUid}.dc");

      Log.LogInformation($"Creating DC file for project {projectUid}");

      File.WriteAllBytes(tempFileName, dcFileContent);

      Log.LogInformation($"Coordiante system file saved as '{tempFileName}'");
    }
  }
}
