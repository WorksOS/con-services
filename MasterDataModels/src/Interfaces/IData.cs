
namespace VSS.MasterData.Models.Interfaces
{
  /// <summary>
  /// Used by master data caching.
  /// </summary>
  public interface IData
  {
    string CacheKey { get; }
  }
}
