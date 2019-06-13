using System;
using System.Text.RegularExpressions;

namespace TestUtility
{
  public static class DateTimeHelper
  {
    /// <summary>
    /// Converts a special date string eg 2d+12:00:00 which signifies a two date and 12 hour offset
    /// to a normal date time based on the first event date.
    /// </summary>
    /// <param name="timeStampAndDayOffSet">Date day off set and timestamp from first event date</param>
    /// <param name="startEventDateTime"></param>
    public static DateTime ConvertTimeStampAndDayOffSetToDateTime(string timeStampAndDayOffSet,DateTime startEventDateTime)
    {
      var components = Regex.Split(timeStampAndDayOffSet, @"d+\+");
      var offset = double.Parse(components[0].Trim());

      return DateTime.Parse(startEventDateTime.AddDays(offset).ToString("yyyy-MM-dd") + " " + components[1].Trim());
    }
  }
}
