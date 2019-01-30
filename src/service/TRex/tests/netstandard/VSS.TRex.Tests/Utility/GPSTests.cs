using System;
using VSS.TRex.Common.Time;
using Xunit;

namespace VSS.TRex.Tests.Utility
{
        public class GPSTests
    {
        [Fact]
        public void Test_GPSOriginTimeToDateTime()
        {
            DateTime dateTime = GPS.GPSOriginTimeToDateTime(10000, 10000000);

            GPS.DateTimeToGPSOriginTime(dateTime, out uint GPSWeek, out uint GPSWeekMilliseconds);

            Assert.True(GPSWeek == 10000 && GPSWeekMilliseconds == 10000000,
                "Round trip conversion of GPS date failed");
        }

        [Fact]
        public void Test_GPSOriginTimeToDateTime_Invalid()
        {
            // Ensure an argument exception occurs for request to convert dates before the GPS origin time
            try
            {
                GPS.DateTimeToGPSOriginTime(new DateTime(1950, 1, 1), out _, out _);

                Assert.True(false,"Date before GPS origin did not cause an exception");
            }
            catch ( ArgumentException )
            {
                // As expected
            }
        }
    }
}
