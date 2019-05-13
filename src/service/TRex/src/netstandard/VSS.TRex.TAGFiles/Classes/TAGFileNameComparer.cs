using System.Collections.Generic;

namespace VSS.TRex.TAGFiles.Classes
{
  public class TAGFileNameComparer : IComparer<string>
  {
    public int Compare(string x, string y)
    {
      // Sort the filename using the date encoded into the filename
      return x.Split('-')[4].CompareTo(y.Split('-')[4]);
    }
  }
}
