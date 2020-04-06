using System.IO;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
        public class TAGHeaderTests : IClassFixture<DILoggingFixture>
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
          using (var reader = new TAGReader(new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile-TAG-Header-Read.tag"), FileMode.Open)))
          {
            Assert.NotNull(reader);

            TAGHeader header = new TAGHeader();

            //Read the header
            header.Read(reader);

            Assert.Equal(1U, header.DictionaryID);
            Assert.Equal(1U, header.DictionaryMajorVer);
            Assert.Equal(4U, header.DictionaryMinorVer);
            Assert.Equal(1U, header.MajorVer);
            Assert.Equal(0U, header.MinorVer);
            Assert.True(
              header.FieldAndTypeTableOffset > 0 && header.FieldAndTypeTableOffset < reader.StreamSizeInNybbles / 2,
              "Field and type table offset read from header is invalid");
          }
        }
    }
}
