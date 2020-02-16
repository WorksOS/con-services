using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
    
    
    /// <summary>
    ///This is a test class for HashUtilsTest and is intended
    ///to contain all HashUtilsTest Unit Tests
    ///</summary>
  [TestClass()]
  public class HashUtilsTest
  {


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
    public void CreateSalt()
    {
      int size = 5; 
      string actual = HashUtils.CreateSalt(size);
      //Can't test equality as different salt each time method called
      Assert.IsNotNull(actual, "Salt should be non null");
      Assert.IsTrue(actual.Length > 0, "Salt should be non-empty");
    }

    [TestMethod()]
    public void ComputeHash()
    {
      string password = "UnitTester";
      string salt = "z+dIgAM=";
      string expected = "90AE42E912BBAF388861953C12258F6AEB5E847C";
      string actual = HashUtils.ComputeHash(password, "SHA1", salt);
      Assert.AreEqual(expected, actual);
    }

  }
}
