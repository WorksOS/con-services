using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap;
using WebApiTests.Executors;

namespace WebApiTests.RadioSerialMap
{
  [TestClass]
  public class CustomRadioSerialMapTests : ExecutorBaseTests
  {
    [TestMethod]
    public void Creation()
    {
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var mapper = new CustomRadioSerialProjectMap(loggerFactory);
      Assert.IsTrue(mapper != null);
    }
  }
}
