using System.Collections.Generic;
using System.Linq;

namespace VSS.Common.Abstractions.Cache.Models
{
  /// <summary>
  /// A class representing a cacheable item
  /// </summary>
  /// <typeparam name="TItem">The Type of item to be cached, must be a reference type</typeparam>
  public class CacheItem<TItem> where TItem : class
  {
    /// <summary>
    /// The value (can be null) of the item to be cached
    /// </summary>
    public TItem Value { get; }

    /// <summary>
    /// Any tags that describe the item cached. Used for invalidation
    /// </summary>
    public List<string> Tags { get; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="value">Value to be cached</param>
    /// <param name="tags">Can be null, which means no tags can be used to clear this item from cache - it must be removed via key or timeout</param>
    public CacheItem(TItem value, IEnumerable<string> tags)
    {
      Value = value;
      Tags = tags?
               .Where(t => !string.IsNullOrEmpty(t))
               .Distinct()
               .ToList()
             ?? new List<string>();
    }
  }
}