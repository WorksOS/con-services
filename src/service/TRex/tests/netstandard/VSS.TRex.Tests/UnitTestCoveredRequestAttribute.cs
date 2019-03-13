using System;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;

namespace VSS.TRex.Tests
{
  /// <summary>
  /// Attribute that describes this unit test class as covering a TRes request
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
  public class UnitTestCoveredRequestAttribute : Attribute
  {
    public Type RequestType { get; set; }
  }
}
