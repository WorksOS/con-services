using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

//Microsoft reference implementation
namespace VSS.Productivity3D.Common.Filters.Caching
{
  internal class MemoryCachedResponse
  {
    public DateTimeOffset Created { get; set; }
    public int StatusCode { get; set; }
    public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
    public List<byte[]> BodySegments { get; set; }
    public long BodyLength { get; set; }
  }
}