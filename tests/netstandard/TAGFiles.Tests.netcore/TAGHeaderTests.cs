using System;
using System.IO;
using VSS.TRex.TAGFiles.Classes;
using VSSTests.TRex.Tests.Common;
using Xunit;

namespace VSS.TRex.TAGFiles.Tests
{
        public class TAGHeaderTests
    {
        [Fact]
        public void Test_TAGHeader_Creation()
        {
            TAGHeader header = new TAGHeader();

            Assert.True(header.DictionaryID == 0 &&
                header.DictionaryMajorVer == 0 &&
                header.DictionaryMinorVer == 0 &&
                header.FieldAndTypeTableOffset == 0 &&
                header.MajorVer == 0 &&
                header.MinorVer == 0,
                "Header not created as expected");
        }

        [Fact]
        public void Test_TAGHeader_Read()
        {
            TAGReader reader = new TAGReader(new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile-TAG-Header-Read.tag"), FileMode.Open));

            Assert.NotNull(reader);

            TAGHeader header = new TAGHeader();

            //Read the header
            header.Read(reader);

            Assert.Equal((uint)1, header.DictionaryID);
            Assert.Equal((uint)1, header.DictionaryMajorVer);
            Assert.Equal((uint)4, header.DictionaryMinorVer);
            Assert.Equal((uint)1, header.MajorVer);
            Assert.Equal((uint)0, header.MinorVer);
            Assert.True(header.FieldAndTypeTableOffset > 0 && header.FieldAndTypeTableOffset < reader.GetSize() / 2,
                          "Field and type table offset read from header is invalid");           
        }
    }
}
