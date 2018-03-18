using System;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using Xunit;

namespace VSS.VisionLink.Raptor.TAGFiles.Tests
{
        public class IntegerFieldNybbleSizesTests
    {
        [Fact]
        public void Test_IntegerFieldNybbleSizes_Sizes()
        {
            Assert.Equal(1, IntegerNybbleSizes.Nybbles(TAGDataType.t4bitInt));
            Assert.Equal(1, IntegerNybbleSizes.Nybbles(TAGDataType.t4bitUInt));

            Assert.Equal(2, IntegerNybbleSizes.Nybbles(TAGDataType.t8bitInt));
            Assert.Equal(2, IntegerNybbleSizes.Nybbles(TAGDataType.t8bitUInt));

            Assert.Equal(3, IntegerNybbleSizes.Nybbles(TAGDataType.t12bitInt));
            Assert.Equal(3, IntegerNybbleSizes.Nybbles(TAGDataType.t12bitUInt));

            Assert.Equal(4, IntegerNybbleSizes.Nybbles(TAGDataType.t16bitInt));
            Assert.Equal(4, IntegerNybbleSizes.Nybbles(TAGDataType.t16bitUInt));

            Assert.Equal(8, IntegerNybbleSizes.Nybbles(TAGDataType.t32bitInt));
            Assert.Equal(8, IntegerNybbleSizes.Nybbles(TAGDataType.t32bitUInt));
        }
    }
}
