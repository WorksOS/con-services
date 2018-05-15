using System;
using System.IO;
using System.Linq;
using VSS.TRex.Utilities.Interfaces;
using VSS.TRex.Utilities.ExtensionMethods;
using Xunit;

namespace VSS.TRex.RaptorClassLibrary.Extensions.Tests
{
        public class FromToBytesTests
    {
        private class TestFromToBytesClass : IBinaryReaderWriter
        {
            public int[] testArray = null; 

            public TestFromToBytesClass()
            {
                testArray = Enumerable.Range(1, 1000).ToArray();
            }

            public void Read(BinaryReader reader)
            {
                testArray = new int[1000];
                for (int i = 0; i < 1000; i++)
                {
                    testArray[i] = reader.ReadInt32();
                }
            }

            public void Write(BinaryWriter writer) => Write(writer, new byte[10000]);

            public void Write(BinaryWriter writer, byte[] buffer)
            {
                foreach (int i in testArray)
                {
                    writer.Write(i);
                }
            }
        }

        /// <summary> 
        /// Ensure the IsEmpty mechanism reports the cell empty of cell passes
        /// </summary>
        [Fact]
        public void Test_FromToBytes()
        {
            TestFromToBytesClass testInstance = new TestFromToBytesClass();
            byte[] toBytes = testInstance.ToBytes();

            TestFromToBytesClass testInstance2 = new TestFromToBytesClass();
            testInstance2.FromBytes(toBytes);

            Assert.True(testInstance.testArray.Zip(testInstance2.testArray, (first, second) => first == second).All(x => x == true),
                          "Sequence of elements not same after FromBytes(ToBytes()) operation");
        }
    }
}