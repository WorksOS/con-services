using System;
using System.Collections.Generic;

namespace VSS.Productivity3D.Common.ResultHandling
{
  public class CompactionDesignProfileResult
  {
      /// <summary>
      /// The design file UID the response data is collected from.
      /// </summary>
      public Guid designFileUid;

      /// <summary>
      /// The collection of vertices produced by the query. Vertices are ordered by increasing station value along the line or alignment.
      /// </summary>
      public List<CompactionProfileVertex> data;
  }
}
