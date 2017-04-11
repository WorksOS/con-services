using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Tests
{
    [TestClass]
    public class TAGDictionaryTests
    {
        [TestMethod]
        public void Test_TAGDictionary_Item_Creation()
        {
            TAGDictionaryItem item = new TAGDictionaryItem("name", TAGDataType.tUnicodeString, 10);

            Assert.IsTrue(item != null && item.Name.Equals("name") && item.Type == TAGDataType.tUnicodeString && item.ID == 10,
                "TAG file dictionary item did not create as expected");
        }

        [TestMethod]
        public void Test_TAGDictionary_Creation()
        {
            TAGDictionary dict = new TAGDictionary();

            Assert.IsTrue(dict != null && dict.Entries != null, "TAG file dictionary did not create as expected");
        }
    
        [TestMethod]
        public void Test_TAGDictionary_Item_Addition()
        {
            TAGDictionary dict = new TAGDictionary();

            dict.Entries.Add(10, new TAGDictionaryItem("name", TAGDataType.tUnicodeString, 10));

            Assert.IsTrue(dict.Entries.Count == 1, "TAG file item did not add as expected");
        }

        [TestMethod]
        public void Test_TAGDictionary_Item_Retrieval()
        {
            TAGDictionary dict = new TAGDictionary();

            dict.Entries.Add(10, new TAGDictionaryItem("name", TAGDataType.tUnicodeString, 10));

            TAGDictionaryItem item = dict.Entries[10];

            Assert.IsTrue(item != null && item.Name.Equals("name") && item.Type == TAGDataType.tUnicodeString && item.ID == 10,
                "TAG file dictionary item did retrieve as expected");
        }

    }
}
