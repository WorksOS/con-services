using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Tests
{
    [TestClass]
    public class IntegerFieldNybbleSizesTests
    {
        [TestMethod]
        public void Test_IntegerFieldNybbleSizes_Sizes()
        {
            Assert.IsTrue(IntegerNybbleSizes.Nybbles(TAGDataType.t4bitInt) == 1, "t4bitInt has incorrect number of nybbles");
            Assert.IsTrue(IntegerNybbleSizes.Nybbles(TAGDataType.t4bitUInt) == 1, "t4bitUInt has incorrect number of nybbles");

            Assert.IsTrue(IntegerNybbleSizes.Nybbles(TAGDataType.t8bitInt) == 2, "t8bitInt has incorrect number of nybbles");
            Assert.IsTrue(IntegerNybbleSizes.Nybbles(TAGDataType.t8bitUInt) == 2, "t8bitUInt has incorrect number of nybbles");

            Assert.IsTrue(IntegerNybbleSizes.Nybbles(TAGDataType.t12bitInt) == 3, "t12bitInt has incorrect number of nybbles");
            Assert.IsTrue(IntegerNybbleSizes.Nybbles(TAGDataType.t12bitUInt) == 3, "t12bitUInt has incorrect number of nybbles");

            Assert.IsTrue(IntegerNybbleSizes.Nybbles(TAGDataType.t16bitInt) == 4, "t16bitInt has incorrect number of nybbles");
            Assert.IsTrue(IntegerNybbleSizes.Nybbles(TAGDataType.t16bitUInt) == 4, "t16bitUInt has incorrect number of nybbles");

            Assert.IsTrue(IntegerNybbleSizes.Nybbles(TAGDataType.t32bitInt) == 8, "t32bitInt has incorrect number of nybbles");
            Assert.IsTrue(IntegerNybbleSizes.Nybbles(TAGDataType.t32bitUInt) == 8, "t32bitUInt has incorrect number of nybbles");
        }
    }
}
