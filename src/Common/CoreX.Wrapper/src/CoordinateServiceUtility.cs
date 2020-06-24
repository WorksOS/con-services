using System.Collections.Generic;
using CoreX.Wrapper.Models;
using CoreX.Wrapper.Types;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Productivity3D.Productivity3D.Models.ProductionData;

namespace CoreX.Wrapper
{
  /// <summary>
  /// Check that the site model exists, and appropriate designs exist.
  /// </summary>
  public class CoordinateServiceUtility : ICoordinateServiceUtility
  {
    private readonly ILogger _log;
    private readonly IConvertCoordinates _convertCoordinates;

    public CoordinateServiceUtility(ILoggerFactory loggerFactory, IConvertCoordinates convertCoordinates)
    {
      _log = loggerFactory.CreateLogger(GetType());
      _convertCoordinates = convertCoordinates;
    }

    /// <inheritdoc/>
    public bool PatchLLH(string csib, List<MachineStatus> machines)
    {
      if (machines.Count == 0)
      {
        return false;
      }

      var xyzCoords = new List<XYZ>();

      foreach (var machine in machines)
      {
        if (machine.lastKnownX != null && machine.lastKnownY != null &&
            machine.lastKnownX != Consts.NULL_DOUBLE && machine.lastKnownY != Consts.NULL_DOUBLE)
        {
          xyzCoords.Add(new XYZ(machine.lastKnownX.Value, machine.lastKnownY.Value, 0.0)); // Note: 2D conversion only, elevation set to 0
        }
      }

      if (xyzCoords.Count > 0)
      {
        var llhCoords = _convertCoordinates.NEEToLLH(csib, xyzCoords.ToArray(), ReturnAs.Degrees);

        // If the count returned is different to that sent, then we can't match with the machines list
        if (xyzCoords.Count == llhCoords.Length)
        {
          var coordPointer = 0;
          foreach (var machine in machines)
          {
            if (machine.lastKnownX != null && machine.lastKnownY != null &&
                machine.lastKnownX != Consts.NULL_DOUBLE && machine.lastKnownY != Consts.NULL_DOUBLE)
            {
              machine.lastKnownLatitude = llhCoords[coordPointer].Y;
              machine.lastKnownLongitude = llhCoords[coordPointer].X;
            }
            coordPointer++;
          }
        }
        else
        {
          var message = $"{nameof(CoordinateServiceUtility)} Failed to convert Coordinates. CSIB: {csib} Coords: {(llhCoords == null ? "null LLH returned" : JsonConvert.SerializeObject(llhCoords))}, NEECoords: {JsonConvert.SerializeObject(xyzCoords)}";
          _log.LogError(message);
          return false;
        }
      }
      else
      {
        _log.LogInformation(
          $"{nameof(CoordinateServiceUtility)} No coordinates need converting as no machines have lastKnownXY. Machines: {JsonConvert.SerializeObject(machines)}");
      }

      return true;
    }
  }
}
