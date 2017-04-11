using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.SubGridTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VSS.VisionLink.Raptor.SubGridTrees.Tests
{
    [TestClass()]
    public class SubGridTreePersistorTests
    {
        [TestMethod()]
        public void Test_SubGridTreePersistor_Write_Empty()
        {
            // Create an empty subgrid bit mask tree and persist it into a stream
            SubGridTreeBitMask masktree = new SubGridTreeBitMask();
            MemoryStream MS = new MemoryStream();

            Assert.IsTrue(SubGridTreePersistor.Write(masktree, "Existance", 1, new BinaryWriter(MS)), "SubGridTreePersistor.Write failed");
            Assert.IsTrue(MS.Length == 38, "Stream length not 38 bytes as expected, is it {0} bytes", MS.Length);
        }

        [TestMethod()]
        public void Test_SubGridTreePersistor_Write_NotEmpty()
        {
            // Create an empty subgrid bit mask tree and persist it into a stream
            SubGridTreeBitMask masktree = new SubGridTreeBitMask();
            masktree.SetCell(100, 100, true);

            MemoryStream MS = new MemoryStream();

            Assert.IsTrue(SubGridTreePersistor.Write(masktree, "Existance", 1, new BinaryWriter(MS)), "SubGridTreePersistor.Write failed");
            Assert.IsTrue(MS.Length == 175, "Stream length not 175 bytes as expected, is it {0} bytes", MS.Length);
        }

        [TestMethod()]
        public void Test_SubGridTreePersistor_Read_Empty()
        {
            // Create an empty subgrid bit mask tree and persist it into a stream, then read it back again
            SubGridTreeBitMask masktree = new SubGridTreeBitMask();
            MemoryStream MS = new MemoryStream();

            Assert.IsTrue(SubGridTreePersistor.Write(masktree, "Existance", 1, new BinaryWriter(MS)), "SubGridTreePersistor.Write failed");

            SubGridTreeBitMask newtree = new SubGridTreeBitMask();

            MS.Position = 0;
            Assert.IsFalse(SubGridTreePersistor.Read(newtree, "ExistanceXXX", 1, new BinaryReader(MS)), "Incorrect header did not cause failure");

            MS.Position = 0;
            Assert.IsFalse(SubGridTreePersistor.Read(newtree, "Existance", 2, new BinaryReader(MS)), "Incorrect version did not cause failure");

            MS.Position = 0;
            Assert.IsTrue(SubGridTreePersistor.Read(newtree, "Existance", 1, new BinaryReader(MS)), "SubGridTreePersistor.Read failed");
        }

        [TestMethod()]
        public void Test_SubGridTreePersistor_Read_NotEmpty()
        {
            // Create an empty subgrid bit mask tree and persist it into a stream
            SubGridTreeBitMask masktree = new SubGridTreeBitMask();
            masktree.SetCell(100, 100, true);

            MemoryStream MS = new MemoryStream();

            Assert.IsTrue(SubGridTreePersistor.Write(masktree, "Existance", 1, new BinaryWriter(MS)), "SubGridTreePersistor.Write failed");

            SubGridTreeBitMask newtree = new SubGridTreeBitMask();

            MS.Position = 0;
            Assert.IsFalse(SubGridTreePersistor.Read(newtree, "ExistanceXXX", 1, new BinaryReader(MS)), "Incorrect header did not cause failure");

            MS.Position = 0;
            Assert.IsFalse(SubGridTreePersistor.Read(newtree, "Existance", 2, new BinaryReader(MS)), "Incorrect version did not cause failure");

            MS.Position = 0;
            Assert.IsTrue(SubGridTreePersistor.Read(newtree, "Existance", 1, new BinaryReader(MS)), "SubGridTreePersistor.Read failed");

            Assert.IsTrue(newtree.CountBits() == 1, "New tree does not have a single bit set, it has {0} bits set", newtree.CountBits());
            Assert.IsTrue(newtree.GetCell(100, 100) == true, "Bit at (100, 100) not set as expected");
        }
    }
}