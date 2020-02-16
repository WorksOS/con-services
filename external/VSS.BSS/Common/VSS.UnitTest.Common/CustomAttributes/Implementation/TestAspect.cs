using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;

namespace VSS.UnitTest.Common._Framework.CustomAttributes.Implementation 
{
  public abstract class TestAspect<TAttribute> : IMessageSink, ITestAspect where TAttribute : Attribute
  {
    private const string TypeNameKey = "__TypeName";
    private const string MethodNameKey = "__MethodName";

    public Type Type { get; protected set; }
    public string MethodName { get; set; }
    public IMessageSink NextSink { get; protected set; }

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
    public void SetNextSink(IMessageSink nextSink)
    {
      NextSink = nextSink;
    }

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
    protected TAttribute GetAttribute(IMessage message)
    {
      string typeName = (string) message.Properties[TypeNameKey];
      MethodName = (string) message.Properties[MethodNameKey];

      Type = Type.GetType(typeName);
      MethodInfo methodInfo = Type.GetMethod(MethodName);

      try
      {
        object[] attributes = methodInfo.GetCustomAttributes(typeof (TAttribute), true);

        foreach (var attribute in attributes)
        {
          TAttribute customAttribute = attribute as TAttribute;
          if (customAttribute != null) return customAttribute;
        }
      } catch (Exception)
      {
        return null;
      }

      return null;
    }

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
    public abstract IMessage SyncProcessMessage(IMessage msg);

    #region NOT IMPLEMENTED

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
    public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
    {
      throw new InvalidOperationException();
    }

    #endregion
  }
}
