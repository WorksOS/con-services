using System;

namespace VSS.TRex.Common.Utilities
{
  [AttributeUsage(AttributeTargets.Assembly)]
  public class ForceAssemblyReference : Attribute
  {
    public ForceAssemblyReference(Type forcedType)
    {
      //not sure if these two lines are required since 
      //the type is passed to constructor as parameter, 
      //thus effectively being used
      void noop(Type _) { }
      noop(forcedType);
    }
  }
}
