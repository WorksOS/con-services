using System;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Types;
using Xunit;

namespace TAGFiles.Tests
{
        public class TAGDictionaryTests
    {
        [Fact]
        public void Test_TAGDictionary_Item_Creation()
        {
            TAGDictionaryItem item = new TAGDictionaryItem("name", TAGDataType.tUnicodeString, 10);

            Assert.True(item != null && item.Name.Equals("name") && item.Type == TAGDataType.tUnicodeString && item.ID == 10,
                "TAG file dictionary item did not create as expected");
        }

        [Fact]
        public void Test_TAGDictionary_Creation()
        {
            TAGDictionary dict = new TAGDictionary();

            Assert.True(dict != null && dict.Entries != null, "TAG file dictionary did not create as expected");
        }
    
        [Fact]
        public void Test_TAGDictionary_Item_Addition()
        {
            TAGDictionary dict = new TAGDictionary();

            dict.Entries.Add(10, new TAGDictionaryItem("name", TAGDataType.tUnicodeString, 10));

            Assert.True(1 == dict.Entries.Count);
        }

        [Fact]
        public void Test_TAGDictionary_Item_Retrieval()
        {
            TAGDictionary dict = new TAGDictionary();

            dict.Entries.Add(10, new TAGDictionaryItem("name", TAGDataType.tUnicodeString, 10));

            TAGDictionaryItem item = dict.Entries[10];

            Assert.True(item != null && item.Name.Equals("name") && item.Type == TAGDataType.tUnicodeString && item.ID == 10,
                "TAG file dictionary item did retrieve as expected");
        }

    }
}
