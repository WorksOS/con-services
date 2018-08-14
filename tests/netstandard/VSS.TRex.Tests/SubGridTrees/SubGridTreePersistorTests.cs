using System.IO;
using System.Text;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Utilities;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
    public class SubGridTreePersistorTests
    {
        [Fact()]
        public void Test_SubGridTreePersistor_Write_Empty()
        {
            // Create an empty subgrid bit mask tree and persist it into a stream
            SubGridTreeSubGridExistenceBitMask masktree = new SubGridTreeSubGridExistenceBitMask();
            MemoryStream MS = new MemoryStream();

            Assert.True(SubGridTreePersistor.Write(masktree, "Existance", 1, new BinaryWriter(MS, Encoding.UTF8, true), null), "SubGridTreePersistor.Write failed");
            Assert.Equal(46, MS.Length);
        }

        [Fact()]
        public void Test_SubGridTreePersistor_Write_NotEmpty()
        {
            // Create an empty subgrid bit mask tree and persist it into a stream
            SubGridTreeSubGridExistenceBitMask masktree = new SubGridTreeSubGridExistenceBitMask();
            masktree.SetCell(100, 100, true);

            MemoryStream MS = new MemoryStream();

            Assert.True(SubGridTreePersistor.Write(masktree, "Existance", 1, new BinaryWriter(MS, Encoding.UTF8, true), null), "SubGridTreePersistor.Write failed");
            Assert.Equal(183, MS.Length);
        }

        [Fact()]
        public void Test_SubGridTreePersistor_Read_Empty()
        {
            // Create an empty subgrid bit mask tree and persist it into a stream, then read it back again
            SubGridTreeSubGridExistenceBitMask masktree = new SubGridTreeSubGridExistenceBitMask();
            MemoryStream MS = new MemoryStream();

            Assert.True(SubGridTreePersistor.Write(masktree, "Existance", 1, new BinaryWriter(MS, Encoding.UTF8, true), null), "SubGridTreePersistor.Write failed");

            SubGridTreeSubGridExistenceBitMask newtree = new SubGridTreeSubGridExistenceBitMask();

            MS.Position = 0;
            Assert.False(SubGridTreePersistor.Read(newtree, "ExistanceXXX", 1, new BinaryReader(MS, Encoding.UTF8, true), null), "Incorrect header did not cause failure");

            MS.Position = 0;
            Assert.False(SubGridTreePersistor.Read(newtree, "Existance", 2, new BinaryReader(MS, Encoding.UTF8, true), null), "Incorrect version did not cause failure");

            MS.Position = 0;
            Assert.True(SubGridTreePersistor.Read(newtree, "Existance", 1, new BinaryReader(MS, Encoding.UTF8, true), null), "SubGridTreePersistor.Read failed");
        }

        [Fact()]
        public void Test_SubGridTreePersistor_Read_NotEmpty()
        {
            // Create an empty subgrid bit mask tree and persist it into a stream
            SubGridTreeSubGridExistenceBitMask masktree = new SubGridTreeSubGridExistenceBitMask();
            masktree.SetCell(100, 100, true);

            MemoryStream MS = new MemoryStream();

            Assert.True(SubGridTreePersistor.Write(masktree, "Existance", 1, new BinaryWriter(MS, Encoding.UTF8, true), null), "SubGridTreePersistor.Write failed");

            SubGridTreeSubGridExistenceBitMask newtree = new SubGridTreeSubGridExistenceBitMask();

            MS.Position = 0;
            Assert.False(SubGridTreePersistor.Read(newtree, "ExistanceXXX", 1, new BinaryReader(MS, Encoding.UTF8, true), null), "Incorrect header did not cause failure");

            MS.Position = 0;
            Assert.False(SubGridTreePersistor.Read(newtree, "Existance", 2, new BinaryReader(MS, Encoding.UTF8, true), null), "Incorrect version did not cause failure");

            MS.Position = 0;
            Assert.True(SubGridTreePersistor.Read(newtree, "Existance", 1, new BinaryReader(MS, Encoding.UTF8, true), null), "SubGridTreePersistor.Read failed");

            Assert.Equal(1, newtree.CountBits());
            Assert.True(newtree.GetCell(100, 100));
        }

      [Fact()]
      public void Test_SubGridTreePersistor_ReadWrite_WithGenericSerialiser()
      {
        // Create an empty subgrid bit mask tree and persist it into a stream, then read it back again
        SubGridTreeSubGridExistenceBitMask masktree = new SubGridTreeSubGridExistenceBitMask();
        MemoryStream MS = new MemoryStream();

        Assert.True(SubGridTreePersistor.Write(masktree, "Existance", 1, new BinaryWriter(MS, Encoding.UTF8, true),
                    (subGrid, writer) => ((SubGridTreeLeafBitmapSubGrid)subGrid).Bits.Write(writer, new byte[10000])), 
          "SubGridTreePersistor.Write failed");

        SubGridTreeSubGridExistenceBitMask newtree = new SubGridTreeSubGridExistenceBitMask();

        MS.Position = 0;
        Assert.False(SubGridTreePersistor.Read(newtree, "ExistanceXXX", 1, new BinaryReader(MS, Encoding.UTF8, true),
          (subGrid, reader) => ((SubGridTreeLeafBitmapSubGrid)subGrid).Bits.Read(reader, new byte[10000])), 
          "Incorrect header did not cause failure");

        MS.Position = 0;
        Assert.False(SubGridTreePersistor.Read(newtree, "Existance", 2, new BinaryReader(MS, Encoding.UTF8, true),
            (subGrid, reader) => ((SubGridTreeLeafBitmapSubGrid)subGrid).Bits.Read(reader, new byte[10000])),
            "Incorrect version did not cause failure");

        MS.Position = 0;
        Assert.True(SubGridTreePersistor.Read(newtree, "Existance", 1, new BinaryReader(MS, Encoding.UTF8, true),
            (subGrid, reader) => ((SubGridTreeLeafBitmapSubGrid)subGrid).Bits.Read(reader, new byte[10000])),
            "SubGridTreePersistor.Read failed");
      }

  }
}
