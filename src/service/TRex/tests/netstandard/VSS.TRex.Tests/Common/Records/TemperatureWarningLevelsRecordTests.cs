using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using VSS.TRex.Common.Records;
using Xunit;

namespace VSS.TRex.Tests.Common.Records
{
  public class TemperatureWarningLevelsRecordTests
  {
    [Fact]
    public void Test_Equals()
    {
      var record1 = new TemperatureWarningLevelsRecord
      {
        Min = 1,
        Max = 11
      };

      var record2 = new TemperatureWarningLevelsRecord
      {
        Min = 1,
        Max = 22
      };

      record1.Equals(record2).Should().BeFalse();
      record1.Equals(record1).Should().BeTrue();
    }

  }
}
