using VSS.VisionLink.Raptor.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VSS.VisionLink.Raptor.Time.Tests
{
        public class GPSTests
    {
        [Fact()]
        public void Test_GPSOriginTimeToDateTime()
        {
            DateTime dateTime = GPS.GPSOriginTimeToDateTime(10000, 10000000);

            uint GPSWeek, GPSWeekMilliseconds;

            GPS.DateTimeToGPSOriginTime(dateTime, out GPSWeek, out GPSWeekMilliseconds);

            Assert.True(GPSWeek == 10000 && GPSWeekMilliseconds == 10000000,
                "Round trip conversion of GPS date failed");
        }

        [Fact()]
        public void Test_GPSOriginTimeToDateTime_Invalid()
        {
            uint GPSWeek, GPSWeekMilliseconds;

            // Ensure an argument exception occurs for request to convert dates before the GPS origin time
            try
            {
                GPS.DateTimeToGPSOriginTime(new DateTime(1950, 1, 1), out GPSWeek, out GPSWeekMilliseconds);

                Assert.True(false,"Dtae before GPS origin did not cuse an exception");
            }
            catch ( ArgumentException )
            {
                // As expected
            }
        }
    }
}