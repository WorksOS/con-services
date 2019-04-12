using System;
using FluentAssertions;
using VSS.TRex.TAGFiles.Classes.ValueMatcher;
using VSS.TRex.Types;
using Xunit;

namespace TAGFiles.Tests
{
        public class AccumulatedAttributesTests
    {
        [Fact]
        public void Test_AccumulatedAttributes_Creation()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();
            Assert.NotNull(attrs);
        }

        [Fact()]
        public void Test_AccumulatedAttributes_DiscardAllButLatest()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            // Add a couple of attributes, check discard preserves the last one
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 1);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 2);

            attrs.DiscardAllButLatest();

            Assert.Equal(1, attrs.NumAttrs);
            Assert.Equal(2, ((int)attrs.GetLatest()));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_Count()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 1);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 2);

            Assert.Equal(2, attrs.NumAttrs);
        }

        [Fact()]
        public void Test_AccumulatedAttributes_Add()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 1);

            Assert.Equal(1, attrs.NumAttrs);
            Assert.Equal(1, (int)(attrs.GetLatest()));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetLatest()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 1);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 2), DateTimeKind.Utc), 2);

            Assert.Equal(2, (int)(attrs.GetLatest()));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetValueAtDateTime_Empty()
        {
          AccumulatedAttributes attrs = new AccumulatedAttributes();

          attrs.GetValueAtDateTime(DateTime.UtcNow, out _).Should().BeFalse();
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 1);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 2);

            object value = null;

            Assert.True(attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 0), DateTimeKind.Utc), out value) && (int) value== 1, 
                          "Failed to locate first attribute with preceding time");

            Assert.True(attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), out value) && (int)value == 1,
                          "Failed to locate first attribute with exact time");

            Assert.True(attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), out value) && (int)value == 1,
                          "Failed to locate first attribute with trailing time");

            Assert.True(attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), out value) && (int)value == 2,
                          "Failed to locate second attribute with exact time");

            Assert.True(attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 20), DateTimeKind.Utc), out value) && (int)value == 2,
                          "Failed to locate second attribute with trailing time");
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetGPSModeAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), GPSMode.Fixed);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), GPSMode.Float);

            Assert.Equal(GPSMode.Fixed, attrs.GetGPSModeAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc)));

            Assert.Equal(GPSMode.Float, attrs.GetGPSModeAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetCCVValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), (short)10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), (short)20);

            Assert.Equal(10, attrs.GetCCVValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc)));

            Assert.Equal(20, attrs.GetCCVValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetRMVValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), (short)10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), (short)20);

            Assert.Equal(10, attrs.GetRMVValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc)));

            Assert.Equal(20, attrs.GetRMVValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetFrequencyValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), (ushort)10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), (ushort)20);

            Assert.Equal(10, attrs.GetFrequencyValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc)));

            Assert.Equal(20, attrs.GetFrequencyValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetAmplitudeValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), (ushort)10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), (ushort)20);

            Assert.Equal(10, attrs.GetAmplitudeValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc)));

            Assert.Equal(20, attrs.GetAmplitudeValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetAgeOfCorrectionValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), (byte)10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), (byte)20);

            Assert.Equal(10, attrs.GetAgeOfCorrectionValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc)));

            Assert.Equal(20, attrs.GetAgeOfCorrectionValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetOnGroundAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), OnGroundState.No);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), OnGroundState.YesLegacy);

            Assert.Equal(OnGroundState.No, attrs.GetOnGroundAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc)));

            Assert.Equal(OnGroundState.YesLegacy, attrs.GetOnGroundAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetMaterialTemperatureValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), (ushort)10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), (ushort)20);

            Assert.Equal(10, attrs.GetMaterialTemperatureValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc)));

            Assert.Equal(20, attrs.GetMaterialTemperatureValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetMDPValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), (short)10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), (short)20);

            Assert.Equal(10, attrs.GetMDPValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc)));

            Assert.Equal(20, attrs.GetMDPValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetMachineSpeedValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), (double)1.0);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), (double)2.0);

            Assert.Equal(1.0, attrs.GetMachineSpeedValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc)));

            Assert.Equal(2.0, attrs.GetMachineSpeedValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetCCAValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), (byte)10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), (byte)20);

            Assert.Equal(10, attrs.GetCCAValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc)));

            Assert.Equal(20, attrs.GetCCAValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc)));
        }

    }
}
