using System;
using VSS.TRex.TAGFiles.Classes;
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
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), 1);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), 2);

            attrs.DiscardAllButLatest();

            Assert.Equal(1, attrs.NumAttrs);
            Assert.Equal(2, ((int)attrs.GetLatest()));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_Count()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), 1);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), 2);

            Assert.Equal(2, attrs.NumAttrs);
        }

        [Fact()]
        public void Test_AccumulatedAttributes_Add()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), 1);

            Assert.Equal(1, attrs.NumAttrs);
            Assert.Equal(1, (int)(attrs.GetLatest()));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetLatest()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), 1);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 2), 2);

            Assert.Equal(2, (int)(attrs.GetLatest()));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), 1);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), 2);

            object value = null;

            Assert.True(attrs.GetValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 0), out value) && (int) value== 1, 
                          "Failed to locate first attribute with preceeding time");

            Assert.True(attrs.GetValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 1), out value) && (int)value == 1,
                          "Failed to locate first attribute with exact time");

            Assert.True(attrs.GetValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5), out value) && (int)value == 1,
                          "Failed to locate first attribute with trailing time");

            Assert.True(attrs.GetValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 10), out value) && (int)value == 2,
                          "Failed to locate second attribute with exact time");

            Assert.True(attrs.GetValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 20), out value) && (int)value == 2,
                          "Failed to locate second attribute with trailing time");
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetGPSModeAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), GPSMode.Fixed);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), GPSMode.Float);

            Assert.Equal(GPSMode.Fixed, attrs.GetGPSModeAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)));

            Assert.Equal(GPSMode.Float, attrs.GetGPSModeAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetCCVValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (short)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (short)20);

            Assert.Equal(10, attrs.GetCCVValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)));

            Assert.Equal(20, attrs.GetCCVValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetRMVValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (short)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (short)20);

            Assert.Equal(10, attrs.GetRMVValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)));

            Assert.Equal(20, attrs.GetRMVValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetFrequencyValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (ushort)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (ushort)20);

            Assert.Equal(10, attrs.GetFrequencyValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)));

            Assert.Equal(20, attrs.GetFrequencyValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetAmplitudeValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (ushort)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (ushort)20);

            Assert.Equal(10, attrs.GetAmplitudeValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)));

            Assert.Equal(20, attrs.GetAmplitudeValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetAgeOfCorrectionValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (byte)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (byte)20);

            Assert.Equal(10, attrs.GetAgeOfCorrectionValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)));

            Assert.Equal(20, attrs.GetAgeOfCorrectionValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetOnGroundAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), OnGroundState.No);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), OnGroundState.YesLegacy);

            Assert.Equal(OnGroundState.No, attrs.GetOnGroundAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)));

            Assert.Equal(OnGroundState.YesLegacy, attrs.GetOnGroundAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetMaterialTemperatureValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (ushort)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (ushort)20);

            Assert.Equal(10, attrs.GetMaterialTemperatureValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)));

            Assert.Equal(20, attrs.GetMaterialTemperatureValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetMDPValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (short)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (short)20);

            Assert.Equal(10, attrs.GetMDPValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)));

            Assert.Equal(20, attrs.GetMDPValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetMachineSpeedValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (double)1.0);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (double)2.0);

            Assert.Equal(1.0, attrs.GetMachineSpeedValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)));

            Assert.Equal(2.0, attrs.GetMachineSpeedValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetCCAValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (byte)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (byte)20);

            Assert.Equal(10, attrs.GetCCAValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)));

            Assert.Equal(20, attrs.GetCCAValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)));
        }

    }
}
