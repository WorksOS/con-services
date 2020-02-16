using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class InputDictionaryTests : BssUnitTestBase
  {
    protected Inputs Inputs = new Inputs();

    [TestMethod]
    public void Get_ValueOfTypeExistsForKey_ReturnTypedValue()
    {
      Inputs.Add<TestType>(new TestType{TestProp = "test prop"});

      var result = Inputs.Get<TestType>();

      Assert.IsInstanceOfType(result, typeof(TestType));
      Assert.AreEqual("test prop", result.TestProp);
    }

    [TestMethod]
    public void Get_TypeDoesNotExist_Throws()
    {
      bool thrown = false;

      try
      {
        Inputs.Get<TestType>();
      }
      catch (Exception ex)
      {
        Assert.IsInstanceOfType(ex, typeof(KeyNotFoundException));
        string expectedMessage = string.Format("InputDictionary does not contain a value for type \"{0}\".", typeof(TestType));
        Assert.AreEqual(expectedMessage, ex.Message);
        thrown = true;
      }

      Assert.IsTrue(thrown);
    }

    [TestMethod]
    public void Get_ValueInDictionaryIsNull_Throws()
    {
      bool thrown = false;

      Inputs.Add<TestType>(null);
      try
      {
        Inputs.Get<TestType>();
      }
      catch (Exception ex)
      {
        Assert.IsInstanceOfType(ex, typeof(ArgumentException));
        string expectedMessage = string.Format("InputDictionary cannot return a non-null value for type \"{0}\".", typeof(TestType));     
        Assert.AreEqual(expectedMessage, ex.Message);
        thrown = true;
      }

      Assert.IsTrue(thrown);
    }

    [TestMethod]
    public void Get_ValueCannotCastToType_Throws()
    {
      bool thrown = false;

      Inputs.Add<TestType>(string.Empty);
      try
      {
        Inputs.Get<TestType>();
      }
      catch (Exception ex)
      {
        Assert.IsInstanceOfType(ex, typeof(InvalidCastException));
        string expectedMessage = string.Format("InputDictionary cannot cast \"{0}\" to \"System.String\".", typeof(TestType));
        Assert.AreEqual(expectedMessage, ex.Message);
        thrown = true;
      }

      Assert.IsTrue(thrown, "Exception not thrown.");
    }
  }

  internal class TestType
  {
    public string TestProp { get; set; }
  }
}
