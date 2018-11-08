using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileHarvester.Implementation;
using VSS.Productivity3D.TagFileHarvester.Models;

namespace VSS.Productivity3D.TagFileHarvesterTests
{

  public static class LockClass
{
    public static readonly object LockObject = new object();
}

  [TestClass]
  public class XMLBookamanagerTests
  {
    readonly static DateTime now = DateTime.Now;
    readonly static string FileToMerge = "<?xml version=\"1.0\" encoding=\"utf-8\"?> " +
                                         "<Bookmarks xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                                         "<KeysAndValues>" +
                                         "<Key>TestOrg6</Key>" +
                                         "<Value>" +
                                         "<BookmarkUTC>2015-07-13T14:55:43.3198922+12:00</BookmarkUTC>" +
                                         "<LastUpdateDateTime>2015-07-13T02:55:43.3668969Z</LastUpdateDateTime>" +
                                         "<LastCycleStartDateTime>0001-01-01T00:00:00</LastCycleStartDateTime>" +
                                         "<LastCycleStopDateTime>0001-01-01T00:00:00</LastCycleStopDateTime>" +
                                         "<OrgIsDisabled>true</OrgIsDisabled>" +
                                         "<CycleLength>0</CycleLength>" +
                                         "<InProgress>false</InProgress>" +
                                         "<LastFilesProcessed>0</LastFilesProcessed>" +
                                         "<LastFilesErrorneous>0</LastFilesErrorneous>" +
                                         "<LastFilesRefused>0</LastFilesRefused>" +
                                         "<OrgName>TestOrg6</OrgName>" +
                                         "<TotalFilesProcessed>0</TotalFilesProcessed>" +
                                         "<TotalFilesErrorneous>0</TotalFilesErrorneous>" +
                                         "<TotalFilesRefused>0</TotalFilesRefused>" +
                                         "</Value>" +
                                         "</KeysAndValues></Bookmarks>";


    [TestInitialize]
    public void InitTests()
    {

      Monitor.Enter(LockClass.LockObject);
      XMLBookMarkManager.Instance.WriteMergeFile(FileToMerge);
      XMLBookMarkManager.Instance.DeleteInstance();
    }

    [TestCleanup]
    public void TestCleanup()
    {
      XMLBookMarkManager.Instance.DeleteFile();
      XMLBookMarkManager.Instance.DeleteMergeFile();
      Monitor.Exit(LockClass.LockObject);
    }

    [TestMethod]
    public void CanCreateXMLBookManager()
    {
      var manager = XMLBookMarkManager.Instance;
      Assert.IsNotNull(manager);
    }

    [TestMethod]
    public void CanAddNewOrgToXMLBookManager()
    {
      var manager = XMLBookMarkManager.Instance;

      manager.UpdateBookmark(new Organization(){filespaceId = "12345", shortName = "TestOrg1", orgId = "abcdef"}, new Bookmark() {BookmarkUTC = now} );
      manager.WriteBookmarks();
      var org = manager.GetBookmark(new Organization() {filespaceId = "12345", shortName = "TestOrg1", orgId = "abcdef"});
      Assert.AreEqual(new Bookmark() { BookmarkUTC = now },org);
    }

   
    [TestMethod]
    public void CanReadSavedDataFromFile()
    {
      var manager = XMLBookMarkManager.Instance;
      manager.UpdateBookmark(new Organization() { filespaceId = "12345", shortName = "TestOrg2", orgId = "uvwxyz" }, new Bookmark() { BookmarkUTC = now });
      manager.WriteBookmarks();
      Thread.Sleep(500);
      XMLBookMarkManager.Instance.DeleteInstance();
      manager = XMLBookMarkManager.Instance;
      var org = manager.GetBookmark(new Organization() { filespaceId = "12345", shortName = "TestOrg2", orgId = "uvwxyz" });
      Assert.AreEqual(new Bookmark() { BookmarkUTC = now }, org);
    }

    [TestMethod]
    public void CanGetBookmarForNonexistentOrg()
    {
      var manager = XMLBookMarkManager.Instance;
      var bm = manager.GetBookmark(new Organization() {filespaceId = "12345", shortName = "TestOrg3", orgId = "mnopqr"});
      Assert.AreEqual(new Bookmark(), bm);
    }

    [TestMethod]
    public void CanGetBookmarkList()
    {
      var log=LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
      var manager = XMLBookMarkManager.Instance;
      XMLBookMarkManager.Log = log;
      manager.UpdateBookmark(new Organization() { filespaceId = "12345", shortName = "TestOrg4",orgId = "hijklm"}, new Bookmark() { BookmarkUTC = now });
      manager.WriteBookmarks();
      manager.StartDataExport();
      Thread.Sleep(1000);
      var result = XMLBookMarkManager.Instance.GetBookmarksList().FirstOrDefault(o => o.OrgName == "TestOrg4");
      manager.StopDataExport();
      Assert.AreEqual("TestOrg4", result.OrgName);
    }

    [TestMethod]
    public void CanUpdateBookmark()
    {
      var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
      var manager = XMLBookMarkManager.Instance;
      XMLBookMarkManager.Log = log;
      manager.UpdateBookmark(new Organization() { filespaceId = "12345", shortName = "TestOrg5", orgId = "qrstuv"}, new Bookmark() { BookmarkUTC = now });
      manager.WriteBookmarks();
      manager.UpdateBookmark(new Organization() { shortName = "TestOrg5"},
          new Bookmark() {BookmarkUTC = now.AddDays(1)});
      var bookmark = manager.GetBookmark(new Organization() { shortName = "TestOrg5" });
      Assert.AreEqual(new Bookmark() { BookmarkUTC = now.AddDays(1) }, bookmark); 
    }

    [TestMethod]
    [Ignore]
    public void CanMergeFiles()
    {
      var manager = XMLBookMarkManager.Instance;
      manager.UpdateBookmark(new Organization() { filespaceId = "12345", shortName = "TestOrg6", orgId ="defghi" }, new Bookmark() { BookmarkUTC = now, OrgIsDisabled = false});
      var newBoms = manager.MergeWithUpdatedBookmarks();
      var bookmark = manager.GetBookmark(new Organization() { filespaceId = "12345", shortName = "TestOrg6", orgId = "defghi" });
      Assert.AreEqual(true,bookmark.OrgIsDisabled);
      Assert.AreEqual(1, newBoms);

    }

  }
}
