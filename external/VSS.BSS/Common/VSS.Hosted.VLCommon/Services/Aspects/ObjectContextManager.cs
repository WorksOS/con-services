using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AopAlliance.Intercept;
using System.Threading;
using System.Reflection;
using System.Transactions;


namespace VSS.Hosted.VLCommon
{
  public class ObjectContextManager : IMethodInterceptor
  {
    public object Invoke(IMethodInvocation invocation)
    {
      Thread.BeginThreadAffinity();

      bool requiresSnapshot = invocation.Method.GetCustomAttributes(typeof(SnapshotAttribute), false) != null;

      SessionContext session = new SessionContext();

      Object res = null;
      try
      {
        session.DisposeNHOpContext();
        
        //IsolationLevel level = requiresSnapshot ? IsolationLevel.Snapshot : IsolationLevel.ReadCommitted;
        //TransactionOptions options = new TransactionOptions{ IsolationLevel=IsolationLevel.Snapshot, Timeout=TimeSpan.FromSeconds(120)};
        //using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
        //{
        //  if (session.NHOpContext is NH_OP)
        //    (session.NHOpContext as NH_OP).Connection.Open();
        //}

        res = invocation.Proceed();
      }
      catch
      {
        throw;
      }
      finally
      {       
        session.DisposeNHOpContext();
        Thread.EndThreadAffinity();

        Thread.SetData(Thread.GetNamedDataSlot("SessionContext"), null);
      }
      return res;
    }
  }
}
