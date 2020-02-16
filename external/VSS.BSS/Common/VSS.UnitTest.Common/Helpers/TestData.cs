using System;

namespace VSS.UnitTest.Common
{
  public class TestData
  {
    [ThreadStatic]
    private static TestDataHelper _testDataHelper;

    public static TestDataHelper Current
    {
      get
      {
        if (_testDataHelper == null)
          _testDataHelper = new TestDataHelper();
        return _testDataHelper;
      }
      set { _testDataHelper = value; }
    }
  }
}
