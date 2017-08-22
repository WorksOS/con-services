using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace VSS.Productivity3D.Common.Filters.Caching
{
    public interface IMemoryCacheBuilder<in T> where T : IEquatable<T>
    {
      IMemoryCache GetMemoryCache(T cacheUid);
      void ClearMemoryCache(T cacheUid); 
    }
}
