using System;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.Contexts
{
  public interface IContextContainer : IDisposable
  {
    INH_OP OpContext { get; }
    INH_OP RawContext { get; }
  }
}