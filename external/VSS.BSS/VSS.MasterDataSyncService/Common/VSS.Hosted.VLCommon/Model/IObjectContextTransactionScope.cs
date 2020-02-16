using System;

namespace VSS.Hosted.VLCommon
{
  public interface IObjectContextTransactionScope : IDisposable
  {
    IDisposable EnrollObjectContexts(params object[] objectContexts);
    void Commit();
  }
}
