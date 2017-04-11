using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using System.IO;

namespace VSS.VisionLink.Raptor.TAGFiles.Tests
{
    [TestClass]
    public class TAGHeaderTests
    {
        [TestMethod]
        public void Test_TAGHeader_Creation()
        {
            TAGHeader header = new TAGHeader();

            Assert.IsTrue(header.DictionaryID == 0 &&
                header.DictionaryMajorVer == 0 &&
                header.DictionaryMinorVer == 0 &&
                header.FieldAndTypeTableOffset == 0 &&
                header.MajorVer == 0 &&
                header.MinorVer == 0,
                "Header not created as expected");
        }

        [TestMethod]
        public void Test_TAGHeader_Read()
        {
            TAGReader reader = new TAGReader(new FileStream(TAGTestConsts.TestDataFilePath() + "TAGFiles\\TestTAGFile-TAG-Header-Read.tag", FileMode.Open));
            //TAGReader reader = new TAGReader(new FileStream(TagTestConsts.TestTAGFileName(), FileMode.Open));

            Assert.IsTrue(reader != null, "Reader failed to construct");

            TAGHeader header = new TAGHeader();

            //Read the header
            header.Read(reader);

            Assert.IsTrue(header.DictionaryID == 1, "Header DictionaryID invalid");
            Assert.IsTrue(header.DictionaryMajorVer == 1, "Header DictionaryMajorVer invalid");
            Assert.IsTrue(header.DictionaryMinorVer == 4, "Header DictionaryMinorVer invalid");
            Assert.IsTrue(header.MajorVer == 1, "Header MajorVer invalid");
            Assert.IsTrue(header.MinorVer == 0, "Header MinorVer invalid");
            Assert.IsTrue(header.FieldAndTypeTableOffset > 0 && header.FieldAndTypeTableOffset < reader.GetSize() / 2,
                          "Field and type table offset read from header is invalid");           
        }
    }
}
