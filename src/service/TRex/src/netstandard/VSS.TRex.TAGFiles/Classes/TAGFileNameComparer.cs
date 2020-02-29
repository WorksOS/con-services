using System.Collections.Generic;

namespace VSS.TRex.TAGFiles.Classes
{
  public class TAGFileNameComparer : IComparer<string>
  {
    /// <summary>
    /// Compares two TAG file names using the standard TAG file naming convention where there is a
    /// date time of the form YYYYMMDDDHHMMSSZZZZ. No format checking of the file names is performed; a
    /// best effort is made to identify the time section of the file name - if the file names do not
    /// conform to the standard naming then the sorted order may not be well defined.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int Compare(string x, string y)
    {
      var cIndexX = x.Length - 1;
      while (cIndexX > 0 && x[cIndexX] != '-')
        cIndexX--;

      var cIndexY = y.Length - 1;
      while (cIndexY > 0 && y[cIndexY] != '-')
        cIndexY--;

      return string.Compare(x, cIndexX, y, cIndexY, x.Length - cIndexX);

      // Sort the filename using the date encoded into the filename.
      // This is functionally equivalent to the code above but involves not string object or array allocations.
      // return x.Split('-')[4].CompareTo(y.Split('-')[4]);
    }
  }
}
