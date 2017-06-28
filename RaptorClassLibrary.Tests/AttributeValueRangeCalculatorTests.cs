using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using VSS.VisionLink.Raptor.SubGridTrees.Server;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Types;
using System.Text;
using System.Linq;
using VSS.VisionLink.Raptor.Compression;

namespace VSS.VisionLink.Raptor.Compression.Tests
{
    [TestClass]
    public class AttributeValueRangeCalculatorTests
    {
        [TestMethod]
        public void Test_AttributeValueRangeCalculator_NullRangeHeights()
        { 
            // Simulate a subgrid contianing the same non-null elevation in every cell, expressed as a single vector
            int value = AttributeValueModifiers.ModifiedHeight(12345.678F);

            int[] values = Enumerable.Range(0, SubGridTree.SubGridTreeCellsPerSubgrid - 1).Select(x => value).ToArray();

            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();

            AttributeValueRangeCalculator.CalculateAttributeValueRange(values, 0xffffffff, 0x7fffffff, true, ref descriptor);

            Assert.IsTrue(descriptor.MinValue == value, "Descriptor minimum value incorrect, should be {0}, is actually {1}", value, descriptor.MinValue);
            Assert.IsTrue(descriptor.MaxValue == value, "Descriptor maximum value incorrect, should be {0}, is actually {1}", value, descriptor.MaxValue);
            Assert.IsTrue(descriptor.RequiredBits == 0, "Descriptor required bit is not 0 (for no entropy), is actually {0}", descriptor.RequiredBits);
        }
    }
}
