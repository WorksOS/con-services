using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Security.Permissions;

namespace VSS.UnitTest.Common._Framework.CustomAttributes.Implementation 
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
  public class UnitTestBaseClassAttribute : ContextAttribute
  {
    private readonly List<Type> _testAspects = new List<Type>();

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
    public UnitTestBaseClassAttribute(params Type[] testAspects) : base("MSTestBaseClass")
    {
      _testAspects.AddRange(testAspects);
    }

    public List<Type> TestAspects { get { return _testAspects; } }

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
    public override void GetPropertiesForNewContext(IConstructionCallMessage msg) 
    {
      if (msg == null)
        throw new ArgumentNullException("msg");

      foreach (Type testAspect in _testAspects)
      {
        Type testPropertyType = typeof(TestProperty<>).MakeGenericType(testAspect);
        msg.ContextProperties.Add(Activator.CreateInstance(testPropertyType));
      }
    }
  }
}
