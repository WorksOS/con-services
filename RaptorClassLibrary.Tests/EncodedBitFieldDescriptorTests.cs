using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Types;
using System.Text;

namespace VSS.VisionLink.Raptor.Compression.Tests
{
    [TestClass]
    public class EncodedBitFieldDescriptorTests
    {
        [TestMethod]
        public void Test_EncodedBitFieldDescriptor_Init()
        {
            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();

            descriptor.Init();

            Assert.IsTrue(descriptor.EncodedNullValue == 0 &&
                descriptor.AllValuesAreNull == false &&
                descriptor.MaxValue == 0 &&
                descriptor.MinValue == 0 &&
                descriptor.NativeNullValue == 0 &&
                descriptor.Nullable == false &&
                descriptor.OffsetBits == 0 &&
                descriptor.RequiredBits == 0,
                "Descriptor state after Init() incorrect");
        }

        [TestMethod]
        public void Test_EncodedBitFieldDescriptor_ReadWrite()
        {
            EncodedBitFieldDescriptor descriptor1 = new EncodedBitFieldDescriptor()
                {
                AllValuesAreNull = false,
                EncodedNullValue = 1235,
                MinValue = 0,
                MaxValue = 1234,
                NativeNullValue = 10000,
                Nullable = true,
                OffsetBits = 1280,
                RequiredBits = 35
            };

            MemoryStream ms = new MemoryStream();

            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true))
            {
                descriptor1.Write(writer);
            }

            EncodedBitFieldDescriptor descriptor2 = new EncodedBitFieldDescriptor();

            ms.Position = 0;
            using (var reader = new BinaryReader(ms, Encoding.UTF8, true))
            {
                descriptor2.Read(reader);
            }

            Assert.AreEqual(descriptor1, descriptor2, "Read after write results in different structures");
        }

        [TestMethod]
        public void Test_EncodedBitFieldDescriptor_CalculateRequiredBitFieldSize()
        {
            // Test case where all values are null (expected 0 bits of entropy)
            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor()
            {
                AllValuesAreNull = true,
            };

            descriptor.CalculateRequiredBitFieldSize();
            Assert.AreEqual(0, descriptor.RequiredBits, "Required bits for 'all null values' field is not zero");

            // Test case where there is 1024 possible values plus an encoded null value, resulting in 10 required bits
            descriptor = new EncodedBitFieldDescriptor()
            {
                AllValuesAreNull = false,
                EncodedNullValue = 1023,
                MinValue = 0,
                MaxValue = 1023,
            };

            descriptor.CalculateRequiredBitFieldSize();
            Assert.AreEqual(10, descriptor.RequiredBits, "Required bits for '1023 values + encoded null' field is incorrect");

            // Test case where there is 1024 possible values plus an encoded null value, resulting in 11 required bits
            descriptor = new EncodedBitFieldDescriptor()
            {
                AllValuesAreNull = false,
                EncodedNullValue = 1024,
                MinValue = 0,
                MaxValue = 1024,
            };

            descriptor.CalculateRequiredBitFieldSize();
            Assert.AreEqual(11, descriptor.RequiredBits, "Required bits for '1024 values + encoded null' field is incorrect");
        }
    }
}