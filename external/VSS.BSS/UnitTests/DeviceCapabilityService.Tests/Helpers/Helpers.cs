using System.Collections.Generic;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests
{
  public static class Helpers
  {
    public static IEnumerable<string> GetTestEndpointNames()
    {
      yield return "CAT";
      yield return "TNL";
    }
  }
}
