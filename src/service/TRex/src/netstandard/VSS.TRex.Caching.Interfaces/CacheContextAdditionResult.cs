using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.Caching.Interfaces
{
  public enum CacheContextAdditionResult
  {
    Unknown,
    Added,
    AlreadyExisting,
    MRUListFull
  }
}
