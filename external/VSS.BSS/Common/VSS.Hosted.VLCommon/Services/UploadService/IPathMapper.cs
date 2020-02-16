using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  public interface IPathMapper
  {
    string MapPath(string sessionID, string relativePath);
    bool Clean { get; }
  }
}
