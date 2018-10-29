using System.IO;

namespace VSS.TRex.Common
{
  /// <summary>
  /// A class to contain a collection of TRex configuration controls.
  /// Should be refactored or modified so the standard c# configuration system is used or underpins it.
  ///
  /// Important! configuration will come from a json file or environment variables so dont use this class. For long term see program.cs for ConfigurationBuilder 
  ///
  /// 
  /// </summary>
  public static class TRexConfig
  {

    /// <summary>
    /// The file system location in which to store Ignite persistent data
    /// </summary>
    public static string PersistentCacheStoreLocation = Path.Combine("/persist", "TRexIgniteData");
    //public static string PersistentCacheStoreLocation = "C:/temp/TRexIgniteData"; //Path.Combine(Path.GetTempPath(), "TRexIgniteData");

  }
}
