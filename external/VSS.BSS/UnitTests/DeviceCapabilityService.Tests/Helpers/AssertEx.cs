using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests
{
  public class AssertEx
  {
    public static void Throws<TException>(Action actionToExecute, string expectedExceptionMessage = null)
    {
      try
      {
        actionToExecute();
      }
      catch (Exception ex)
      {
        if (ex.GetType() != typeof (TException))
        {
          string incorrectExceptionThrownMessage =
            string.Format("Expected exception of type: {0}, but exception of type: {1} was thrown.",
                          typeof (TException).Name, ex.GetType().Name);
          Assert.Fail(incorrectExceptionThrownMessage);
        }

        if (expectedExceptionMessage != null &&
            ex.Message.IndexOf(expectedExceptionMessage, StringComparison.InvariantCultureIgnoreCase) < 0)
        {
          string exceptionMessageNotMatchedMessage = string.Format(
            "Exception message: \"{0}\" did not contain \"{1}\".", ex.Message, expectedExceptionMessage);
          Assert.Fail(exceptionMessageNotMatchedMessage);
        }

        return;
      }

      string exceptionNotThrownMessage = string.Format("Expected exception of type: {0}, but no exception was thrown.",
                                                       typeof (TException).Name);
      Assert.Fail(exceptionNotThrownMessage);
    }
  }
}