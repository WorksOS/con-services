using System;
using System.Runtime.Remoting.Messaging;

namespace VSS.UnitTest.Common._Framework.CustomAttributes.Implementation
{
  public interface ITestAspect
  {
    Type Type { get; }
    string MethodName { get; }

    void SetNextSink(IMessageSink nextSink);
  }
}