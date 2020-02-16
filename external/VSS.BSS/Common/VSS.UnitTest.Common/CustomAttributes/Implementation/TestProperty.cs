using System;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;

namespace VSS.UnitTest.Common._Framework.CustomAttributes.Implementation 
{
  public class TestProperty<T> : IContextProperty, IContributeObjectSink where T : IMessageSink, ITestAspect, new()
  {
    public bool IsNewContextOK(Context newCtx)
    {
      return true;
    }

    public string Name
    {
      get { return typeof(T).AssemblyQualifiedName; }
    }

    public IMessageSink GetObjectSink(MarshalByRefObject obj, IMessageSink nextSink)
    {
      T testAspect = new T();
      testAspect.SetNextSink(nextSink);
      return testAspect;
    }
    
    public void Freeze(Context newContext) {}
  }
}
