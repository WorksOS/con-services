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
      return !(uTMCoordPointPair.Left.X == Consts.NullReal || uTMCoordPointPair.Left.Y == Consts.NullReal || uTMCoordPointPair.Right.X == Consts.NullReal || uTMCoordPointPair.Right.Y == Consts.NullReal);
    }

    public List<UTMCoordPointPair> TranslatePositions(string projectCSIBFile, List<UTMCoordPointPair> coordPositions)
    {
      // testing only
      var DIMENSIONS_2012_WITHOUT_VERT_ADJUST = "VE5MIENTSUIAAAAAAAAmQFByb2plY3Rpb24gZnJvbSBkYXRhIGNvbGxlY3RvcgAAWm9uZSBmcm9tIGRhdGEgY29sbGVjdG9yAABab25lIGZyb20gZGF0YSBjb2xsZWN0b3IAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEBAABEYXR1bVRocmVlUGFyYW1ldGVycwAAAAAAAABAplRYQeM2GhTEP1hBAAAAAAAAAIAAAAAAAAAAgAAAAAAAAACADlpvbmUgZnJvbSBkYXRhIGNvbGxlY3RvcgAAt0YpjXZY6L8DXnvbAh4IQAAAAAAAaihBAAAAAABqGEEAAAAAAADwPwAAAAAAAPA/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFab25lIGZyb20gZGF0YSBjb2xsZWN0b3IAAIF4LavCiChB4TsitpOdF0Fw85mH0Tt/vw9Pj9e7aCk/tdtew3Jyyj4+8FHdAgDwPwFab25lIGZyb20gZGF0YSBjb2xsZWN0b3IAAHbj3dWkgShBru4e94uPF0FcFrRN6LYnwNAqy4rBHPC+JEubOpUlrr4AABgtRFT7Ifm/GC1EVPshCUAYLURU+yH5PxgtRFT7IQlAAQEBAQEBA1AAcgBvAGoAZQBjAHQAaQBvAG4AIABmAHIAbwBtACAAZABhAHQAYQAgAGMAbwBsAGwAZQBjAHQAbwByAAAAWgBvAG4AZQAgAGYAcgBvAG0AIABkAGEAdABhACAAYwBvAGwAbABlAGMAdABvAHIAAABaAG8AbgBlACAAZgByAG8AbQAgAGQAYQB0AGEAIABjAG8AbABsAGUAYwB0AG8AcgAAAAAARABhAHQAdQBtAFQAaAByAGUAZQBQAGEAcgBhAG0AZQB0AGUAcgBzAAAAAABaAG8AbgBlACAAZgByAG8AbQAgAGQAYQB0AGEAIABjAG8AbABsAGUAYwB0AG8AcgAAAAAAAABaAG8AbgBlACAAZgByAG8AbQAgAGQAYQB0AGEAIABjAG8AbABsAGUAYwB0AG8AcgAAAFoAbwBuAGUAIABmAHIAbwBtACAAZABhAHQAYQAgAGMAbwBsAGwAZQBjAHQAbwByAAAAAAAAAAAAAAB7TEdFPTQxNjc7TERFPTYxNjc7R0dFPTQxNjc7R0RFPTYxNjc7fQB7TEVFPTcwMTk7fQAA";

      // test only
      projectCSIBFile = DIMENSIONS_2012_WITHOUT_VERT_ADJUST;

      if (projectCSIBFile == string.Empty)
      {
        Log.LogError($"TranslatePositions. Missing project CSIB file.");
        return null;
      }

      if (coordPositions == null || coordPositions.Count == 0) return coordPositions;  // nothing todo

      try
      {

        var convertCoordinates = DIContext.Obtain<ICoreXWrapper>();

        if (convertCoordinates == null)
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
            //    currentUTMCSIBFile = convertCoordinates.GetUTMZone(currentUTMZone);
            currentUTMCSIBFile = DIMENSIONS_2012_WITHOUT_VERT_ADJUST; // debugging
          }

          if (ValidPositionsforPair(coordPositions[i]))
          {
            // convert left point to WGS84 LL point
            var leftLLPoint = convertCoordinates.NEEToLLH(currentUTMCSIBFile, coordPositions[i].Left.ToCoreX_XYZ()).ToTRex_XYZ();
            // convert left WGS84 LL point to project NNE
            var leftNNEPoint = convertCoordinates.LLHToNEE(projectCSIBFile, leftLLPoint.ToCoreX_XYZ(), CoreX.Types.InputAs.Radians).ToTRex_XYZ();

            // convert right point to WGS84 LL point
            var rightLLPoint = convertCoordinates.NEEToLLH(currentUTMCSIBFile, coordPositions[i].Right.ToCoreX_XYZ()).ToTRex_XYZ();
            // convert right WGS84 LL point to project NNE
            var rightNNEPoint = convertCoordinates.LLHToNEE(projectCSIBFile, rightLLPoint.ToCoreX_XYZ(), CoreX.Types.InputAs.Radians).ToTRex_XYZ();

            coordPositions[i] = new UTMCoordPointPair(leftNNEPoint, rightNNEPoint, currentUTMZone);
          }

        }

      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Exception occurred while converting ACS coordinates");
        return null;
      }

      return coordPositions;
    }
  }
}
