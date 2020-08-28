using System;
using System.Collections.Generic;
using CoreX.Interfaces;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Geometry;

namespace VSS.TRex.TAGFiles.Executors
{
  /// <summary>
  /// Using the CoreX library covert UTM coordinates into WSG84LL coords, then back to project coordinates.  
  /// </summary>
  public class ACSTranslator : IACSTranslator
  {

    private static readonly ILogger Log = Logging.Logger.CreateLogger<ACSTranslator>();

    private bool ValidPositionsforPair(UTMCoordPointPair uTMCoordPointPair)
    {
      return !(uTMCoordPointPair.Left.X == Consts.NullReal || uTMCoordPointPair.Left.Y == Consts.NullReal || uTMCoordPointPair.Left.Z == Consts.NullReal || uTMCoordPointPair.Right.X == Consts.NullReal || uTMCoordPointPair.Right.Y == Consts.NullReal || uTMCoordPointPair.Right.Z == Consts.NullReal);
    }

    public List<UTMCoordPointPair> TranslatePositions(string projectCSIBFile, List<UTMCoordPointPair> coordPositions)
    {
      // testing only waiting on new corex wrapper
      var utm13 = "VE5MIENTSUIAAAAAAAAmQFdvcmxkIHdpZGUvVVRNAAAxMyBOb3J0aAAAMTMgTm9ydGgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAABXR1MgMTk4NAAAV29ybGQgR2VvZGV0aWMgU3lzdGVtIDE5ODQAAAAAAECmVFhB0ZccFMQ/WEEOMTMgTm9ydGgAAAAAAAAAAAAA8olP4k9S/b8AAAAAAAAAAAAAAACAhB5BeJyiI7n87z8AAAAAAADwPwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA8L8uvbtK4K1ZwAAAAAAAQFVALr27SuCtV8ABAQEBAQEDVwBvAHIAbABkACAAdwBpAGQAZQAvAFUAVABNAAAAMQAzACAATgBvAHIAdABoAAAAMQAzACAATgBvAHIAdABoAAAAAABXAEcAUwAgADEAOQA4ADQAAABXAG8AcgBsAGQAIABHAGUAbwBkAGUAdABpAGMAIABTAHkAcwB0AGUAbQAgADEAOQA4ADQAAAAxADMAIABOAG8AcgB0AGgAAAAAAAAAAAAAAAAAAAAAAAAAe0xHRT00MzI2O0xERT02MzI2O0dHRT00MzI2O0dERT02MzI2O30Ae0xFRT03MDMwO30AAA==";
      var DIMENSIONS_2012_DC_CSIB = "VE5MIENTSUIAAAAAAAAmQFNDUzkwMCBMb2NhbGl6YXRpb24AAFNDUzkwMCBSZWNvcmQAAFNDUzkwMCBSZWNvcmQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEBAABEYXR1bVRocmVlUGFyYW1ldGVycwAAAABEBABAplRYQZPeGxTEP1hBAAAAAAAAAIAAAAAAAAAAgAAAAAAAAACADlNDUzkwMCBSZWNvcmQAAPwzidi3OOQ/A9VI04kPAMCtCIFm/n6RQMxN+n31I6FAVC6H71oA8D8AAAAAAADwPwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABU0NTOTAwIFJlY29yZAAAJZmoeTC7kkDzZo8993ajQECCedBUzGU/2wH4yFX6IT/4iVvMj3UBP/+MJqUNAPA/AVNDUzkwMCBSZWNvcmQAAHxLxEIKf5FAZq5nO/IjoUCxjjO4UgNNQCU1m2Q3dRY/doMN9PUhhD4AABgtRFT7Ifm/GC1EVPshCUAYLURU+yH5PxgtRFT7IQlAAQEBAQEBA1MAQwBTADkAMAAwACAATABvAGMAYQBsAGkAegBhAHQAaQBvAG4AAABTAEMAUwA5ADAAMAAgAFIAZQBjAG8AcgBkAAAAUwBDAFMAOQAwADAAIABSAGUAYwBvAHIAZAAAAAAARABhAHQAdQBtAFQAaAByAGUAZQBQAGEAcgBhAG0AZQB0AGUAcgBzAAAAAABTAEMAUwA5ADAAMAAgAFIAZQBjAG8AcgBkAAAAAAAAAFMAQwBTADkAMAAwACAAUgBlAGMAbwByAGQAAABTAEMAUwA5ADAAMAAgAFIAZQBjAG8AcgBkAAAAAAAAAAAAAAAAAAA=";
      projectCSIBFile = DIMENSIONS_2012_DC_CSIB;

      if (projectCSIBFile == string.Empty)
      {
        Log.LogError($"TranslatePositions. Missing project CSIB file.");
        return null;
      }

      if (coordPositions == null || coordPositions.Count == 0) return coordPositions;  // nothing todo

      try
      {

        var coreXWrapper = DIContext.Obtain<ICoreXWrapper>();
        if (coreXWrapper == null)
        {
          Log.LogError("TranslatePositions. IConvertCoordinates not implemented");
          return null;
        }

        byte currentUTMZone = 0;
        var currentUTMCSIBFile = string.Empty;

        for (var i = 0; i < coordPositions.Count; i++)
        {

          if (coordPositions[i].UTMZone != currentUTMZone || currentUTMCSIBFile == string.Empty)
          {
            currentUTMZone = coordPositions[i].UTMZone;
            //   currentUTMCSIBFile = coreXWrapper.GetUTMZone(currentUTMZone);
            currentUTMCSIBFile = utm13; // debugging
          }

          if (ValidPositionsforPair(coordPositions[i]))
          {
            // convert left point to WGS84 LL point
            var leftLLPoint = coreXWrapper.NEEToLLH(currentUTMCSIBFile, coordPositions[i].Left.ToCoreX_XYZ()).ToTRex_XYZ();
            if (leftLLPoint.IsZeroed())
            {
              // CoreX functions can fail slientlty and return a zeroed XYZ. For conversions to Lat Long Elev we can safely check to make sure there is has been a successful conversion
              Log.LogError($"TranslatePositions. Failed NEEToLLH conversion for ACS coordinates LeftPoint{leftLLPoint}");
              return null;
            }
            // convert left WGS84 LL point to project NNE
            var leftNNEPoint = coreXWrapper.LLHToNEE(projectCSIBFile, leftLLPoint.ToCoreX_XYZ(), CoreX.Types.InputAs.Radians).ToTRex_XYZ();

            // convert right point to WGS84 LL point
            var rightLLPoint = coreXWrapper.NEEToLLH(currentUTMCSIBFile, coordPositions[i].Right.ToCoreX_XYZ()).ToTRex_XYZ();
            if (rightLLPoint.IsZeroed())
            {
              Log.LogError($"TranslatePositions. Failed NEEToLLH conversion for ACS coordinates. RightPoint {rightLLPoint}");
              return null;
            }

            // convert right WGS84 LL point to project NNE
            var rightNNEPoint = coreXWrapper.LLHToNEE(projectCSIBFile, rightLLPoint.ToCoreX_XYZ(), CoreX.Types.InputAs.Radians).ToTRex_XYZ();

            coordPositions[i] = new UTMCoordPointPair(leftNNEPoint, rightNNEPoint, currentUTMZone);
          }

        }

      }
      catch (Exception ex)
      {
        Log.LogError(ex, $"Exception occurred while converting ACS coordinates. {ex.Message}" );
        return null;
      }

      return coordPositions;
    }
  }
}
