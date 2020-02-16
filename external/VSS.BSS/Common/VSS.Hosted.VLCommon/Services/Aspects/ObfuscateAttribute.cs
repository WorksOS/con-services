using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
  public class ObfuscateAttribute : Attribute
  {
    //Either ArgumentName or ArgumentIndex should be used. Web API uses name, client services uses index
    public string ArgumentName { get; set; }
    public int ArgumentIndex { get; set; }
    //Leave this null if argument is a primitive type otherwise it's the property of the argument object which is to be obfuscated
    public string PropertyName { get; set; }
  }
}
