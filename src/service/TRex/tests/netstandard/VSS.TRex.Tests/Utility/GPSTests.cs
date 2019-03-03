using System;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Time;
using Xunit;

namespace VSS.TRex.Tests.Utility
{
    public class GPSTests
    {
        [Fact]
        public void GPSOriginTimeToDateTime()
        {
            DateTime dateTime = GPS.GPSOriginTimeToDateTime(10000, 10000000);

            GPS.DateTimeToGPSOriginTime(dateTime, out uint GPSWeek, out uint GPSWeekMilliseconds);

            Assert.True(GPSWeek == 10000 && GPSWeekMilliseconds == 10000000,
                "Round trip conversion of GPS date failed");
        }

        [Fact]
        public void DateTimeToGPSOriginTime_Invalid_Early()
        {
            Action act = () => GPS.DateTimeToGPSOriginTime(new DateTime(1950, 1, 1), out _, out _);
            act.Should().Throw<ArgumentException>().WithMessage("Date to be converted to GPS date is before the GPS date origin*");
        }

        [Fact]
        public void GPSOriginTimeToDateTime_Invalid_MilliSecondsInWeek()
        {
          Action act = () => GPS.GPSOriginTimeToDateTime(1000, 1_000_000_000);
          act.Should().Throw<TRexException>().WithMessage("GPS millisecondsInWeek: * not in range 0*");
        }

        [Fact]
        public void GetLocalGMTOffset()
        {
          GPS.GetLocalGMTOffset().Should().Be(TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow));
        }

        [Fact]
        public void GetLocalGMTOffset_AtOffset()
        {
          var dateTime = DateTime.UtcNow;
          GPS.GetLocalGMTOffset(dateTime).Should().Be(TimeZoneInfo.Local.GetUtcOffset(dateTime));
        }
  }
}
