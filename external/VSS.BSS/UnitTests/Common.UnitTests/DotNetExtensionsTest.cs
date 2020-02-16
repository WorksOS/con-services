using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;
using System.Collections.Generic;
using System.Xml.Linq;

namespace UnitTests
{
    
    
    /// <summary>
    ///This is a test class for DotNetExtensionsTest and is intended
    ///to contain all DotNetExtensionsTest Unit Tests
    ///</summary>
  [TestClass()]
  public class DotNetExtensionsTest
  {


    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    #region Additional test attributes
    // 
    //You can use the following additional attributes as you write your tests:
    //
    //Use ClassInitialize to run code before running the first test in the class
    //[ClassInitialize()]
    //public static void MyClassInitialize(TestContext testContext)
    //{
    //}
    //
    //Use ClassCleanup to run code after all tests in a class have run
    //[ClassCleanup()]
    //public static void MyClassCleanup()
    //{
    //}
    //
    //Use TestInitialize to run code before running each test
    //[TestInitialize()]
    //public void MyTestInitialize()
    //{
    //}
    //
    //Use TestCleanup to run code after each test has run
    //[TestCleanup()]
    //public void MyTestCleanup()
    //{
    //}
    //
    #endregion


    [TestMethod()]
    public void KeyDateTest()
    {
      //Jan 1 2000
      DateTime value = new DateTime(2000,1,1); 
      int expected = 20000101; 
      int actual;
      actual = DotNetExtensions.KeyDate(value);
      Assert.AreEqual(expected, actual,"The KeyDate extension method is not working");

      //Dec 31 2000
      value = new DateTime(2000, 12, 31);
      expected = 20001231;
      actual = DotNetExtensions.KeyDate(value);
      Assert.AreEqual(expected, actual, "The KeyDate extension method is not working");

      //Apr 15 2000
      value = new DateTime(2000, 4, 15);
      expected = 20000415;
      actual = DotNetExtensions.KeyDate(value);
      Assert.AreEqual(expected, actual, "The KeyDate extension method is not working");

      //Dec 31 2010
      value = new DateTime(2010, 12, 31);
      expected = 20101231;
      actual = DotNetExtensions.KeyDate(value);
      Assert.AreEqual(expected, actual, "The KeyDate extension method is not working");
    }

    [TestMethod()]
    public void NullKeyDateTest()
    {
      DateTime? nullDate = null;

      int keyDate = nullDate.KeyDate();
      Assert.AreEqual(DotNetExtensions.NullKeyDate, keyDate, " A null key date is represented as 99991231");
    }

    [TestMethod()]
    public void FromKeyDateTest()
    {
      int goodKeyDate = 20111006;

      DateTime dt = goodKeyDate.FromKeyDate();
      Assert.AreEqual(2011, dt.Year, "Woops parsed the year wrong");
      Assert.AreEqual(10, dt.Month, "Woops parsed the month wrong");
      Assert.AreEqual(6, dt.Day, "Woops parsed the day wrong");

    }

    [TestMethod()]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void BadFromKeyDateTest()
    {
      int badKeyDate = 101;

      DateTime dt = badKeyDate.FromKeyDate();
    }

    [TestMethod()]
    public void KeyDateYearTest()
    {
      int goodKeyDate = 20111006;

      int year = goodKeyDate.KeyDateYear();
      Assert.AreEqual(2011, year, "Woops parsed the year wrong");

    }
    [TestMethod()]
    public void KeyDateMonthTest()
    {
      int goodKeyDate = 20111006;

      int part = goodKeyDate.KeyDateMonth();
      Assert.AreEqual(10, part, "Woops parsed the keydate wrong");

    }
    [TestMethod()]
    public void KeyDateDayTest()
    {
      int goodKeyDate = 20111006;

      int part = goodKeyDate.KeyDateDay();
      Assert.AreEqual(6, part, "Woops parsed the keydate wrong");
    }

    [TestMethod()]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void BadKeyDateYearTest()
    {
      int badKeyDate = -32;
      int part = badKeyDate.KeyDateYear();
    }
    [TestMethod()]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void BadKeyDateMonthTest()
    {
      int badKeyDate = -32;
      int part = badKeyDate.KeyDateYear();
    }
    [TestMethod()]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void BadKeyDateDayTest()
    {
      int badKeyDate = -32;
      int part = badKeyDate.KeyDateYear();
    }

    [TestMethod()]
    public void TestKeyDateToIso8601Date_Success()
    {
      int keyDate = 20121024;
      Assert.AreEqual("2012-10-24", keyDate.KeydateToIso8601Date());
    }

    [TestMethod()]
    public void TestKeyDateToIso8601Date_OneDigitMonth_PaddedWithZero_Success()
    {
      int keyDate = 20120124;
      Assert.AreEqual("2012-01-24", keyDate.KeydateToIso8601Date());
    }

    [TestMethod()]
    public void TestKeyDateToIso8601Date_OneDigitDay_PaddedWithZero_Success()
    {
      int keyDate = 20121004;
      Assert.AreEqual("2012-10-04", keyDate.KeydateToIso8601Date());
    }

    [TestMethod()]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestKeyDateToIso8601Date_BadDate_ThrowsException()
    {
      int badKeyDate = -32;
      badKeyDate.KeydateToIso8601Date();
    }

    [TestMethod()]
    public void ContainsIgnoreCaseTest()
    {
      string container = "ABCDEF";
      string contained = "cde";
      string notcontained = "foo";

      Assert.AreEqual(true, container.ContainsIgnoreCase(contained), "String contains ignore case function fails, not finding {0} in {1}", contained, container);
      Assert.AreEqual(false, container.ContainsIgnoreCase(notcontained), "String contains ignore case function fails, errantly finding {0} in {1}", notcontained, container);

      contained = "aBc";
      notcontained = "0AB";
      Assert.AreEqual(true, container.ContainsIgnoreCase(contained), "String contains ignore case function fails, not finding {0} in {1}", contained, container);
      Assert.AreEqual(false, container.ContainsIgnoreCase(notcontained), "String contains ignore case function fails, errantly finding {0} in {1}", notcontained, container);

      contained = "DEF";
      notcontained = "EFg";
      Assert.AreEqual(true, container.ContainsIgnoreCase(contained), "String contains ignore case function fails, not finding {0} in {1}", contained, container);
      Assert.AreEqual(false, container.ContainsIgnoreCase(notcontained), "String contains ignore case function fails, errantly finding {0} in {1}", notcontained, container);
    }


    /// <summary>
    ///A test for GetFirstNotNullFromTheParameters
    ///</summary>
    [TestMethod()]
    public void GetFirstNotNullFromTheParametersTest()
    {
      string actual;
      actual = DotNetExtensions.GetFirstNotNullFromTheParameters(null, "", "abc", "def");
      Assert.AreEqual("abc", actual);
      actual = DotNetExtensions.GetFirstNotNullFromTheParameters(null, null,"");
      Assert.AreEqual(string.Empty, actual);
      Assert.AreEqual(actual.IndexOf("E"),-1);
    }



    [TestMethod]
    public void GetUTCDateTimeAttributeExact_WithMilliseconds()
    {
      XElement xml = XElement.Parse("<thing timestamp=\"2014-09-09T16:50:49.587Z\" />");
      DateTime? actual = xml.GetUTCDateTimeAttributeExact("timestamp");
      DateTime expected = DateTime.Parse("2014-09-09 16:50:49");

      Assert.IsNotNull(actual);
      Assert.AreEqual(expected.Year, actual.Value.Year);
      Assert.AreEqual(expected.Month, actual.Value.Month);
      Assert.AreEqual(expected.Day, actual.Value.Day);
      Assert.AreEqual(expected.Hour, actual.Value.Hour);
      Assert.AreEqual(expected.Minute, actual.Value.Minute);
      Assert.AreEqual(expected.Second, actual.Value.Second);
    }

    [TestMethod]
    public void GetUTCDateTimeAttributeExact_WithManyMilliseconds()
    {
      XElement xml = XElement.Parse("<thing timestamp=\"2014-09-09T16:50:49.583467Z\" />");
      DateTime? actual = xml.GetUTCDateTimeAttributeExact("timestamp");
      DateTime expected = DateTime.Parse("2014-09-09 16:50:49");

      Assert.IsNotNull(actual);
      Assert.AreEqual(expected.Year, actual.Value.Year);
      Assert.AreEqual(expected.Month, actual.Value.Month);
      Assert.AreEqual(expected.Day, actual.Value.Day);
      Assert.AreEqual(expected.Hour, actual.Value.Hour);
      Assert.AreEqual(expected.Minute, actual.Value.Minute);
      Assert.AreEqual(expected.Second, actual.Value.Second);
    }

    [TestMethod]
    public void GetUTCDateTimeAttributeExact_WithoutMilliseconds()
    {
      XElement xml = XElement.Parse("<thing timestamp=\"2014-09-09T16:50:49Z\" />");
      DateTime? actual = xml.GetUTCDateTimeAttributeExact("timestamp");
      DateTime expected = DateTime.Parse("2014-09-09 16:50:49");

      Assert.IsNotNull(actual);
      Assert.AreEqual(expected.Year, actual.Value.Year);
      Assert.AreEqual(expected.Month, actual.Value.Month);
      Assert.AreEqual(expected.Day, actual.Value.Day);
      Assert.AreEqual(expected.Hour, actual.Value.Hour);
      Assert.AreEqual(expected.Minute, actual.Value.Minute);
      Assert.AreEqual(expected.Second, actual.Value.Second);
    }




    [TestMethod]
    public void StringTruncateToLengthTests()
    {
      string actual;

      actual = "12345678".TruncateToLength(8);
      Assert.AreEqual("12345678", actual);

      actual = "12345678".TruncateToLength(7);
      Assert.AreEqual("1234567", actual);

      actual = "12345678".TruncateToLength(9);
      Assert.AreEqual("12345678", actual);

      string nullString = null;
      actual = nullString.TruncateToLength(9);
      Assert.IsNull(actual);

      actual = "".TruncateToLength(4);
      Assert.AreEqual("", actual);

      actual = "12345678".TruncateToLength(0);
      Assert.AreEqual("", actual);

      actual = "12345678".TruncateToLength(-1);
      Assert.AreEqual("12345678", actual);
    }

    [TestMethod]
    public void TypeOrNullableTypeTests()
    {
      Assert.AreEqual(typeof(int), typeof(int).GetTypeOrNullableType());
      Assert.AreEqual(typeof(int), typeof(int?).GetTypeOrNullableType());
      Assert.AreEqual(typeof(int), typeof(Nullable<int>).GetTypeOrNullableType());
      Assert.AreEqual(typeof(DateTime), typeof(DateTime?).GetTypeOrNullableType());
      Assert.AreEqual(typeof(string), typeof(string).GetTypeOrNullableType());
    }

    [TestMethod]
    public void ImplementsGenericInterfaceTests()
    {
      Assert.IsTrue(typeof(List<string>).ImplementsGenericInterface(typeof(ICollection<>)));
      Assert.IsFalse(typeof(string).ImplementsGenericInterface(typeof(ICollection<>)));
      Assert.IsFalse(typeof(int).ImplementsGenericInterface(typeof(ICollection<>)));
    }

    [TestMethod]
    public void HasDefaultConstructorTests()
    {
      Assert.IsTrue(typeof(EventArgs).HasDefaultConstructor());
      Assert.IsTrue(typeof(List<string>).HasDefaultConstructor());
      Assert.IsFalse(typeof(string).HasDefaultConstructor());
    }

    [TestMethod]
    public void InvokeDefaultConstructorTests()
    {
      object target1 = typeof(EventArgs).InvokeDefaultConstructor();
      Assert.IsNotNull(target1);
      Assert.IsInstanceOfType(target1, typeof(EventArgs));

      object target2 = typeof(List<string>).InvokeDefaultConstructor();
      Assert.IsNotNull(target2);
      Assert.IsInstanceOfType(target2, typeof(List<string>));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void InvokeDefaultConstructor_NoDefaultConstructor()
    {
      object target1 = typeof(string).InvokeDefaultConstructor();
    }

		[TestMethod]
	  public void EnumerableExtensionChunk_ChunkIntoTenPieces_ReturnsAll()
		{
			var list = Enumerable.Range(0, 100);
			var chunkAggregate = new List<int>();
			foreach(var chunk in list.Chunk(10))
				chunkAggregate.AddRange(chunk);

			Assert.AreEqual(list.Count(), chunkAggregate.Count);
			Assert.AreEqual(list.First(), chunkAggregate.First());
			Assert.AreEqual(list.Last(), chunkAggregate.Last());
		}
  }
}
