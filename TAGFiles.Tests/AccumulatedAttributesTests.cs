using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Tests
{
    [TestClass]
    public class AccumulatedAttributesTests
    {
        [TestMethod]
        public void Test_AccumulatedAttributes_Creation()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();
            Assert.IsTrue(attrs != null, "Failed to create instance");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_DiscardAllButLatest()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            // Add a couple of attributes, check discard preserves the last one
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), 1);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), 2);

            attrs.DiscardAllButLatest();

            Assert.IsTrue(attrs.NumAttrs == 1, "Discard did not result in a single attribute remaining");
            Assert.IsTrue(((int)attrs.GetLatest()) == 2, "Discard did not preserve the latest element");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_Count()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), 1);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), 2);

            Assert.IsTrue(attrs.NumAttrs == 2, "Attribute count should be 2");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_Add()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), 1);

            Assert.IsTrue(attrs.NumAttrs == 1, "Number of attributes is incorrect");
            Assert.IsTrue((int)(attrs.GetLatest()) == 1, "Value of latest added attribute is incorrect");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetLatest()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), 1);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 2), 2);

            Assert.IsTrue((int)(attrs.GetLatest()) == 2, "Value of latest added attribute is incorrect");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), 1);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), 2);

            object value = null;

            Assert.IsTrue(attrs.GetValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 0), out value) && (int) value== 1, 
                          "Failed to locate first attribute with preceeding time");

            Assert.IsTrue(attrs.GetValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 1), out value) && (int)value == 1,
                          "Failed to locate first attribute with exact time");

            Assert.IsTrue(attrs.GetValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5), out value) && (int)value == 1,
                          "Failed to locate first attribute with trailing time");

            Assert.IsTrue(attrs.GetValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 10), out value) && (int)value == 2,
                          "Failed to locate second attribute with exact time");

            Assert.IsTrue(attrs.GetValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 20), out value) && (int)value == 2,
                          "Failed to locate second attribute with trailing time");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetGPSModeAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), GPSMode.Fixed);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), GPSMode.Float);

            Assert.IsTrue(attrs.GetGPSModeAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)) == GPSMode.Fixed,
                          "Failed to locate correct attribute value");

            Assert.IsTrue(attrs.GetGPSModeAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)) == GPSMode.Float,
                          "Failed to locate correct attribute value");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetCCVValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (short)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (short)20);

            Assert.IsTrue(attrs.GetCCVValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)) == 10,
              "Failed to locate correct attribute value");

            Assert.IsTrue(attrs.GetCCVValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)) == 20,
                          "Failed to locate correct attribute value");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetRMVValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (short)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (short)20);

            Assert.IsTrue(attrs.GetRMVValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)) == 10,
              "Failed to locate correct attribute value");

            Assert.IsTrue(attrs.GetRMVValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)) == 20,
                          "Failed to locate correct attribute value");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetFrequencyValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (ushort)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (ushort)20);

            Assert.IsTrue(attrs.GetFrequencyValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)) == 10,
              "Failed to locate correct attribute value");

            Assert.IsTrue(attrs.GetFrequencyValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)) == 20,
                          "Failed to locate correct attribute value");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetAmplitudeValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (ushort)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (ushort)20);

            Assert.IsTrue(attrs.GetAmplitudeValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)) == 10,
              "Failed to locate correct attribute value");

            Assert.IsTrue(attrs.GetAmplitudeValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)) == 20,
                          "Failed to locate correct attribute value");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetAgeOfCorrectionValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (byte)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (byte)20);

            Assert.IsTrue(attrs.GetAgeOfCorrectionValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)) == 10,
              "Failed to locate correct attribute value");

            Assert.IsTrue(attrs.GetAgeOfCorrectionValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)) == 20,
                          "Failed to locate correct attribute value");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetOnGroundAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), OnGroundState.No);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), OnGroundState.YesLegacy);

            Assert.IsTrue(attrs.GetOnGroundAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)) == OnGroundState.No,
              "Failed to locate correct attribute value");

            Assert.IsTrue(attrs.GetOnGroundAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)) == OnGroundState.YesLegacy,
                          "Failed to locate correct attribute value");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetMaterialTemperatureValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (ushort)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (ushort)20);

            Assert.IsTrue(attrs.GetMaterialTemperatureValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)) == 10,
              "Failed to locate correct attribute value");

            Assert.IsTrue(attrs.GetMaterialTemperatureValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)) == 20,
                          "Failed to locate correct attribute value");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetMDPValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (short)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (short)20);

            Assert.IsTrue(attrs.GetMDPValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)) == 10,
                          "Failed to locate correct attribute value");

            Assert.IsTrue(attrs.GetMDPValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)) == 20,
                          "Failed to locate correct attribute value");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetMachineSpeedValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (double)1.0);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (double)2.0);

            Assert.IsTrue(attrs.GetMachineSpeedValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)) == 1.0,
              "Failed to locate correct attribute value");

            Assert.IsTrue(attrs.GetMachineSpeedValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)) == 2.0,
                          "Failed to locate correct attribute value");
        }

        [TestMethod()]
        public void Test_AccumulatedAttributes_GetCCAValueAtDateTime()
        {
            AccumulatedAttributes attrs = new AccumulatedAttributes();

            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 1), (byte)10);
            attrs.Add(new DateTime(2000, 1, 1, 1, 1, 10), (byte)20);

            Assert.IsTrue(attrs.GetCCAValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 5)) == 10,
                          "Failed to locate correct attribute value");

            Assert.IsTrue(attrs.GetCCAValueAtDateTime(new DateTime(2000, 1, 1, 1, 1, 15)) == 20,
                          "Failed to locate correct attribute value");
        }

    }
}
