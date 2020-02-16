using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class InvalidActionTests : BssUnitTestBase
  {
    [TestMethod]
    public void Execute_ReturnsBssErrorResultWithFailureCode()
    {
      var result = new InvalidAction(ActionEnum.Swapped, typeof(AccountHierarchy)).Execute(null);
      
      Assert.IsInstanceOfType(result, typeof(BssErrorResult));
      var bssErrorResult = (BssErrorResult) result;

      Assert.AreEqual(BssFailureCode.ActionInvalid, bssErrorResult.FailureCode);
      string message = string.Format(BssConstants.ACTION_INVALID_FOR_MESSAGE, ActionEnum.Swapped, typeof(AccountHierarchy).Name);
      Assert.AreEqual(message, bssErrorResult.Summary);
    }
  }
}
