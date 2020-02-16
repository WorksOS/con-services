using System;

namespace VSS.Hosted.VLCommon
{
  public class ObjectContextTransactionParams<T>
  {
    public ObjectContextTransactionParams(T callback, IObjectContextTransactionScope scope)
    {
      if(null == callback)
      {
        throw new ArgumentNullException("callback");
      }
      if(null == scope)
      {
        throw new ArgumentNullException("scope");
      }
      Callback = callback;
      Scope = scope;
    }

    public T Callback { get; private set; }
    public IObjectContextTransactionScope Scope { get; private set; }
  }
}
