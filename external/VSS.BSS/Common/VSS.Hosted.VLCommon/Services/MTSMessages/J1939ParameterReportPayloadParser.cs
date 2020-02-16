using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public static class J1939ParameterReportPayloadParser
  {
    //private static List<J1939ParameterParser> parser;

    private static void Init()
    {
      //parser = new List<J1939ParameterParser>();
      //using (INH_RAW rawCtx = ObjectContextFactory.NewNHContext<INH_RAW>())
      //{
      //  parser = (from p in rawCtx.J1939ParameterParserReadOnly
      //               select p).ToList();

      //}
    }

    public static int GetUnitTypeID(int pgn, int spn, int sourceAddress)
    {
      //try
      //{
      //  if (parser == null || parser.Count == 0)
      //    Init();

      //  J1939ParameterParser parse = parser.Where(e => e.PGN == pgn && e.SPN == spn && (e.SourceAddress == -1 || e.SourceAddress == sourceAddress)).SingleOrDefault();
      //  if (parse != null)
      //    return parse.fk_UnitTypeID;
      //}
      //catch (Exception) { }
      return 0;
    }

    public static int PositionInString(int pgn, int spn, int sourceAddress)
    {
      //try
      //{
      //  if (parser == null || parser.Count == 0)
      //    Init();

      //  J1939ParameterParser parse = parser.Where(e => e.PGN == pgn && e.SPN == spn && (e.SourceAddress == -1 || e.SourceAddress == sourceAddress)).SingleOrDefault();
      //  if (parse != null)
      //    return parse.Length.Value;
      //}
      //catch (Exception) { }
      return 0;
    }
  }
}
