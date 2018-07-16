using System;
using System.Collections.Generic;
using System.Linq;
using VSS.TRex.Events;
using Xunit;

namespace VSS.TRex.Tests.netcore.Events
{
    public class StartEndRecordedDataEventsTests
    {
        [Fact]
        public void Test_StartEndRecordedDataEvents_Creation()
        {
            StartEndProductionEvents events = new StartEndProductionEvents(null, -1, Guid.Empty,
                ProductionEventType.StartEndRecordedData,
                (w, s) => w.Write((byte)s),
                r => (ProductionEventType)r.ReadByte());

            Assert.True(null != events, "Failed to create events list");
            Assert.True(ProductionEventType.StartEndRecordedData == events.EventListType, "Incorrect event list type");
            Assert.True(0 == events.Events.Count, "New list is not empty");
            Assert.True(-1 == events.MachineID, "Machine ID not -1");
            Assert.True(Guid.Empty == events.SiteModelID, "Site model ID is not null");
            Assert.True(null == events.Container, "Container is not null");
            Assert.True(null != events.SerialiseStateIn, "SerialiseStateIn is null");
            Assert.True(null != events.SerialiseStateOut, "SerialiseStateOut is null");
        }

        [Fact]
        public void Test_StartEndRecordedDataEvents_SimpleStartEndAndCollation()
        {
            StartEndProductionEvents events =
                new StartEndProductionEvents(null, -1, Guid.Empty, ProductionEventType.StartEndRecordedData, null, null);

            var firstEventDate = new DateTime(2000, 1, 1, 0, 0, 0);
            var secondEventDate = new DateTime(2000, 1, 1, 1, 0, 0);

            void CheckEvents()
            {
                Assert.True(firstEventDate == events.Events[0].Date, $"Date of first element incorrect, expected {firstEventDate}, got {events.Events[0].Date}");
                Assert.True(ProductionEventType.StartEvent == events.Events[0].State, $"State of first element incorrect, expected {ProductionEventType.StartEvent}, got {events.Events[0].State}");

                Assert.True(secondEventDate == events.Events[1].Date, $"Date of second element incorrect, expected {secondEventDate}, got {events.Events[1].Date}");
                Assert.True(ProductionEventType.EndEvent == events.Events[1].State, $"State of second element incorrect, expected {ProductionEventType.EndEvent}, got {events.Events[1].State}");
            }

            // Add a single start and end at different dates and ensure they are both present and ordered correctly, before and after collation
            events.PutValueAtDate(firstEventDate, ProductionEventType.StartEvent);
            events.PutValueAtDate(secondEventDate, ProductionEventType.EndEvent);

            Assert.True(2 == events.Count(), $"List contains {events.Count()} events, instead of 2");

            // Check the state is good
            CheckEvents();

            // Collate the events and ensure nothing changes
            events.Collate();

            // Check the state is still good
            CheckEvents();
        }

        public static DateTime outerFirstEventDate = new DateTime(2000, 1, 1, 0, 0, 0);
        public static DateTime innerFirstEventDate = new DateTime(2000, 1, 1, 0, 10, 0);
        public static DateTime innerSecondEventDate = new DateTime(2000, 1, 1, 0, 20, 0);
        public static DateTime outerSecondEventDate = new DateTime(2000, 1, 1, 1, 0, 0);

        public static IEnumerable<object[]> InnerOuterEventDates(int numTests)
        {
            var allData = new List<object[]>
            {
                new object[] {outerFirstEventDate, innerFirstEventDate, innerSecondEventDate, outerSecondEventDate},
                new object[] {outerFirstEventDate, outerFirstEventDate, outerSecondEventDate, outerSecondEventDate}
            };

            return allData.Take(numTests);
        }

        [Theory]
        [MemberData(nameof(InnerOuterEventDates), parameters: 4)]
        public void Test_StartEndRecordedDataEvents_DuplicatedAndNestedStartEndAndCollation
            (DateTime outerFirstEventDate, DateTime innerFirstEventDate,
            DateTime innerSecondEventDate, DateTime outerSecondEventDate)
        {
            StartEndProductionEvents events =
                new StartEndProductionEvents(null, -1, Guid.Empty, ProductionEventType.StartEndRecordedData, null, null);

            void CheckEventsBefore(int count)
            {
                if (count >= 1)
                {
                    Assert.True(outerFirstEventDate == events.Events[0].Date,
                        $"Count {count}: Date of outer first element incorrect, expected {outerFirstEventDate}, got {events.Events[0].Date}");
                    Assert.True(ProductionEventType.StartEvent == events.Events[0].State,
                        $"Count {count}: State of outer first element incorrect, expected {ProductionEventType.StartEvent}, got {events.Events[0].State}");
                }

                if (count >= 2)
                {
                    Assert.True(innerFirstEventDate == events.Events[1].Date,
                        $"Count {count}: Date of inner first element incorrect, expected {innerFirstEventDate}, got {events.Events[1].Date}");
                    Assert.True(ProductionEventType.StartEvent == events.Events[1].State,
                        $"Count {count}: State of inner first element incorrect, expected {ProductionEventType.StartEvent}, got {events.Events[1].State}");
                }

                if (count >= 3)
                {
                    Assert.True(innerSecondEventDate == events.Events[2].Date,
                        $"Count {count}: Date of inner second element incorrect, expected {innerSecondEventDate}, got {events.Events[2].Date}");
                    Assert.True(ProductionEventType.EndEvent == events.Events[2].State,
                        $"Count {count}: State of inner second element incorrect, expected {ProductionEventType.EndEvent}, got {events.Events[2].State}");
                }

                if (count >= 4)
                {
                    Assert.True(outerSecondEventDate == events.Events[3].Date,
                        $"Count {count}: Date of outer second element incorrect, expected {outerSecondEventDate}, got {events.Events[3].Date}");
                    Assert.True(ProductionEventType.EndEvent == events.Events[3].State,
                        $"Count {count}: State of outer second element incorrect, expected {ProductionEventType.EndEvent}, got {events.Events[3].State}");
                }
            }

            void CheckEventsAfter()
            {
                Assert.True(outerFirstEventDate == events.Events[0].Date, $"Date of first outer element incorrect, expected {outerFirstEventDate}, got {events.Events[0].Date}");
                Assert.True(ProductionEventType.StartEvent == events.Events[0].State, $"State of first outer element incorrect, expected {ProductionEventType.StartEvent}, got {events.Events[0].State}");

                Assert.True(outerSecondEventDate == events.Events[1].Date, $"Date of second outer element incorrect, expected {outerSecondEventDate}, got {events.Events[1].Date}");
                Assert.True(ProductionEventType.EndEvent == events.Events[1].State, $"State of second outer element incorrect, expected {ProductionEventType.EndEvent}, got {events.Events[1].State}");
            }

            // Add nested start and end events at different dates and ensure they are both present and ordered correctly, before and after collation
            events.PutValueAtDate(outerFirstEventDate, ProductionEventType.StartEvent);
            CheckEventsBefore(1);
            events.PutValueAtDate(innerFirstEventDate, ProductionEventType.StartEvent);
            CheckEventsBefore(2);
            events.PutValueAtDate(innerSecondEventDate, ProductionEventType.EndEvent);
            CheckEventsBefore(3);
            events.PutValueAtDate(outerSecondEventDate, ProductionEventType.EndEvent);
            CheckEventsBefore(4);

            Assert.True(4 == events.Count(), $"List contains {events.Count()} events, instead of 4");

            // Check the four elements are as expected
            CheckEventsBefore(4);

            // Collate the events and ensure nothing changes
            events.Collate();

            // Check the resulting 2 elements are as expected
            CheckEventsAfter();
        }
    }
}
