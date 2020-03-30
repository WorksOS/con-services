using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace IntegrationTests
{
  public class TestTest
  {
    [Fact]
    public void Test()
    {
      var x = TestUtility.RdKafkaDriver.SendKafkaMessage("bob", "bob");
    }
  }
}
