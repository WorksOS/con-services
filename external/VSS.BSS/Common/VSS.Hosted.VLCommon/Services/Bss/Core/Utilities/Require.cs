using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class Require
  {
    public static void IsNotNull<T>(T arg, string argName) where T : class
    {
      if(arg != null)
        return;

      if (string.IsNullOrWhiteSpace(argName))
        argName = string.Format("Argument of type {0}",  typeof (T).Name);

      throw new InvalidOperationException(argName + " cannot be null."); //Purposefully not ArgNullException for message
    }
  }
}