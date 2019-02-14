using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.Tests
{
  [TestClass]
  public class JobRequestTests
  {
    [TestMethod]
    public void ValidateJobRequestSuccess()
    {
      var request = new JobRequest {JobUid = Guid.NewGuid()};
      request.Validate();
    }

    [TestMethod]
    public void ValidateJobRequestFailure()
    {
      var request = new JobRequest();
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }
  }
}
