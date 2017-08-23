using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Practices.ObjectBuilder2;
using VSS.Productivity3D.TagFileHarvester.Interfaces;
using VSS.Productivity3D.TagFileHarvester.Models;
using VSS.Productivity3D.TagFileHarvester.TaskQueues;

namespace VSS.Productivity3D.TagFileHarvester.Implementation
{

  public class XMLBookMarkManager : IBookmarkManager
  {
    private Bookmarks bookmarks;
    private Bookmarks bookmarksToMerge;
    public static string filename;
    public static readonly string filenameToMerge = OrgsHandler.BookmarkPath + "\\update_bookmarks.xml";
    private static readonly LimitedConcurrencyLevelTaskScheduler bkmschdl = new LimitedConcurrencyLevelTaskScheduler(1);
    private static readonly TaskFactory factory = new TaskFactory(bkmschdl);


    public static ILog Log;

    private static XMLBookMarkManager self;
    private static readonly object lockContext = new object();

    private static Thread WriteThread;


    public static XMLBookMarkManager Instance
    {
      get
      { 
        return self ?? (self = new XMLBookMarkManager());
      }
    }

    public IEnumerable<Bookmark> GetBookmarksList()
    {
      Mutex mutex = null;
      try
      {
        mutex = Mutex.OpenExisting(@"Global\XMLBookmarkTagHarvester_MMF_IPC", MutexRights.Synchronize);
        mutex.WaitOne();
        var mmf = MemoryMappedFile.OpenExisting(@"Global\XMLBookmarkTagHarvester", MemoryMappedFileRights.Read);
        {



          using (var stream = mmf.CreateViewStream(0,0,MemoryMappedFileAccess.Read))
          {
            using (BinaryReader binReader = new BinaryReader(stream))
            {
              var length = binReader.ReadInt64();
              MemoryStream ms = new MemoryStream(binReader.ReadBytes((int)length));
              System.Xml.Serialization.XmlSerializer reader =
                  new System.Xml.Serialization.XmlSerializer(typeof (Bookmarks));
              bookmarks = new Bookmarks();
              bookmarks = (Bookmarks) reader.Deserialize(ms);
            }
          }

        }
      }
      finally
      {
        if (mutex != null)
          mutex.ReleaseMutex();
      }

      foreach (var bookmark in bookmarks.KeysAndValues)
        {
          bookmark.Value.OrgName = bookmark.Key;
          yield return bookmark.Value;
        }

    }

    public void DeleteFile()
    {
      lock (lockContext)
      {
        if (File.Exists(filename))
          File.Delete(filename);
      }
    }

    public void DeleteMergeFile()
    {
      lock (lockContext)
      {
        if (File.Exists(filenameToMerge))
          File.Delete(filenameToMerge);
      }
    }

    public void WriteMergeFile(string contents)
    {
      lock (lockContext)
      {
        File.WriteAllText(filenameToMerge,contents);
      }
      
    }

    public int MergeWithUpdatedBookmarks()
    {
      bookmarksToMerge = new Bookmarks();
      System.IO.StreamReader file;
      try
      {
        lock (lockContext)
        {
          if (!File.Exists(filenameToMerge)) return 0;
          System.Xml.Serialization.XmlSerializer reader =
              new System.Xml.Serialization.XmlSerializer(typeof (Bookmarks));
          using (file = new System.IO.StreamReader(filenameToMerge))
          {
            bookmarksToMerge = new Bookmarks();
            bookmarksToMerge = (Bookmarks) reader.Deserialize(file);
            bookmarksToMerge.ToDictionary()
                .ForEach(b => SetBookmarkUTC(new Organization() {shortName = b.Key}, b.Value.BookmarkUTC));
          }
           File.Move(filenameToMerge, DateTime.UtcNow.Ticks+filenameToMerge);
        }
      }
      catch 
      {
        //empty bookmarks here - no file found. Starting from the very beginning
        return 0;
      }
      return bookmarksToMerge.ToDictionary().Count;
    }

    private XMLBookMarkManager()
    {
      filename=OrgsHandler.BookmarkPath + "\\bookmarks.xml";
      try
      {
        lock (lockContext)
        {
          System.Xml.Serialization.XmlSerializer reader =
              new System.Xml.Serialization.XmlSerializer(typeof (Bookmarks));
          using (System.IO.StreamReader file = new System.IO.StreamReader(filename))
          {
            bookmarks = new Bookmarks();
            bookmarks = (Bookmarks) reader.Deserialize(file);
          }
        }
      }
      catch (Exception)
      {
        //empty bookmarks here - no file found. Starting from the very beginning
        bookmarks = new Bookmarks();
      }
    }


    public Bookmark GetBookmark(Organization org)
    {
      lock (lockContext)
      {

        if (!bookmarks.ToDictionary().ContainsKey(org.shortName))
          UpdateBookmark(org, new Bookmark());
        return bookmarks.ToDictionary().FirstOrDefault(b => b.Key == org.shortName).Value;
      }
    }

    public void UpdateBookmark(Organization org, Bookmark bookmark)
    {
      //Make this thread safe.
      lock (lockContext)
      {
        if (bookmarks.ToDictionary().ContainsKey(org.shortName))
          Bookmark.AssignBookmark(bookmark,bookmarks.ToDictionary()[org.shortName]);
        else
          bookmarks.KeysAndValues.Add(new DictionaryProxy<string, Bookmark>.KeyAndValue()
                                      {
                                          Key = org.shortName,
                                          Value = bookmark
                                      });

      }
      SetBookmarkLastUpdateDateTime(org, DateTime.UtcNow);
      WriteBookmarksAsync();
    }

    public Task WriteBookmarksAsync()
    {
      return factory.StartNew(() =>
                                   {
                                     System.Xml.Serialization.XmlSerializer writerInt =
                                         new System.Xml.Serialization.XmlSerializer(typeof (Bookmarks));

                                     lock (lockContext)
                                     {
                                       using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, false))
                                       {
                                         writerInt.Serialize(file, bookmarks);
                                         file.Close();
                                       }
                                     }
                                   });
    }


    private void WriteData(object cBookmark)
    {
      Bookmarks bookmarks = cBookmark as Bookmarks;
      bool mutexCreated;
      MutexSecurity msec = new MutexSecurity();
      msec.AddAccessRule(new MutexAccessRule("everyone",MutexRights.FullControl, AccessControlType.Allow));
      Mutex mutex = new Mutex(false, @"Global\XMLBookmarkTagHarvester_MMF_IPC", out mutexCreated,msec);
      bool exceptionRaised = false;

      

      while (!exceptionRaised)
      {
        try
        {
          try
          {
            mutex.WaitOne();
              Log.Debug("Exporting MMF");
            MemoryStream ms = null;
            lock (lockContext)
            {
              System.Xml.Serialization.XmlSerializer writer =
                  new System.Xml.Serialization.XmlSerializer(typeof (Bookmarks));

               ms = new MemoryStream();
              writer.Serialize(ms, bookmarks);
            }

              MemoryMappedFileSecurity CustomSecurity = new MemoryMappedFileSecurity();
              CustomSecurity.AddAccessRule(new System.Security.AccessControl.AccessRule<MemoryMappedFileRights>(
                  "everyone", MemoryMappedFileRights.FullControl, System.Security.AccessControl.AccessControlType.Allow));
              var mmf = MemoryMappedFile.CreateOrOpen(@"Global\XMLBookmarkTagHarvester", ms.Length+sizeof(long),
                    MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None, CustomSecurity,
                    System.IO.HandleInheritability.Inheritable);
              using (var accessor = mmf.CreateViewAccessor(0, ms.Length + sizeof(long), MemoryMappedFileAccess.ReadWrite))
              {
                // Write to MMF
                accessor.Write(0,ms.Length);
                accessor.WriteArray<byte>(sizeof(long), ms.ToArray(), 0, (int)ms.Length);
                accessor.Flush();
              }
          }
          catch (Exception ex)
          {
            Log.DebugFormat("Exporting MMF Exception occured {0}",ex.Message);
            exceptionRaised = true;
          }
          finally
          {
            mutex.ReleaseMutex();
          }
          Thread.Sleep(5000);
        }
        catch (Exception ex)
        {
          Log.DebugFormat("Exporting MMF Exception occured {0}", ex.Message);
          exceptionRaised = true;
        }
      }
    }

    public void StartDataExport()
    {
     lock (lockContext)
      {
          WriteThread = new Thread(WriteData);
          WriteThread.Priority = ThreadPriority.AboveNormal;
          WriteThread.Start(bookmarks);
      }
    }


    public void StopDataExport()
    {
      lock (lockContext)
      {
        if (WriteThread == null) return;
        if (!WriteThread.IsAlive) return;
        if (WriteThread.IsAlive)
          WriteThread.Abort();
      }
    }

    public IBookmarkManager WriteBookmarks()
    {

      WriteBookmarksAsync().Wait();
      return this;
    }

    public IBookmarkManager SetBookmarkUTC(Organization org, DateTime utcNow)
    {
      //Make this thread safe.
      lock (lockContext)
      {
        if (bookmarks.ToDictionary().ContainsKey(org.shortName))
          this.bookmarks.ToDictionary()[org.shortName].BookmarkUTC = utcNow;
        else
          bookmarks.KeysAndValues.Add(new DictionaryProxy<string, Bookmark>.KeyAndValue()
          {
            Key = org.shortName,
            Value = new Bookmark() { BookmarkUTC = utcNow}
          });
      }
      SetBookmarkLastUpdateDateTime(org, DateTime.UtcNow);
      return this;
    }

    public IBookmarkManager SetBookmarkLastTCCScanTimeUTC(Organization org, DateTime utcNow)
    {
      //Make this thread safe.
      lock (lockContext)
      {
        if (bookmarks.ToDictionary().ContainsKey(org.shortName))
          this.bookmarks.ToDictionary()[org.shortName].LastTCCScanDateTime = utcNow;
        else
          bookmarks.KeysAndValues.Add(new DictionaryProxy<string, Bookmark>.KeyAndValue()
          {
            Key = org.shortName,
            Value = new Bookmark() { LastTCCScanDateTime = utcNow }
          });
      }
      SetBookmarkLastUpdateDateTime(org, DateTime.UtcNow);
      return this;
    }

    public IBookmarkManager SetBookmarkInProgress(Organization org, bool value)
    {
      //Make this thread safe.
      lock (lockContext)
      {
        if (bookmarks.ToDictionary().ContainsKey(org.shortName))
          this.bookmarks.ToDictionary()[org.shortName].InProgress = value;
        else
          bookmarks.KeysAndValues.Add(new DictionaryProxy<string, Bookmark>.KeyAndValue()
          {
            Key = org.shortName,
            Value = new Bookmark() { InProgress = value }
          });
      }
      SetBookmarkLastUpdateDateTime(org, DateTime.UtcNow);
      return this;
    }



    public IBookmarkManager SetBookmarkLastFilesProcessed(Organization org, int value)
    {
      //Make this thread safe.
      lock (lockContext)
      {
        if (bookmarks.ToDictionary().ContainsKey(org.shortName))
          this.bookmarks.ToDictionary()[org.shortName].LastFilesProcessed = value;
        else
          bookmarks.KeysAndValues.Add(new DictionaryProxy<string, Bookmark>.KeyAndValue()
          {
            Key = org.shortName,
            Value = new Bookmark() { LastFilesProcessed = value }
          });
      }
      SetBookmarkLastUpdateDateTime(org, DateTime.UtcNow);
      return this;
    }

    public IBookmarkManager SetBookmarkLastFilesRefused(Organization org, int value)
    {
      //Make this thread safe.
      lock (lockContext)
      {
        if (bookmarks.ToDictionary().ContainsKey(org.shortName))
          this.bookmarks.ToDictionary()[org.shortName].LastFilesRefused = value;
        else
          bookmarks.KeysAndValues.Add(new DictionaryProxy<string, Bookmark>.KeyAndValue()
          {
            Key = org.shortName,
            Value = new Bookmark() { LastFilesRefused = value }
          });
      }
      SetBookmarkLastUpdateDateTime(org, DateTime.UtcNow);
      return this;
    }

    public IBookmarkManager SetBookmarkLastFilesError(Organization org, int value)
    {
      //Make this thread safe.
      lock (lockContext)
      {
        if (bookmarks.ToDictionary().ContainsKey(org.shortName))
          this.bookmarks.ToDictionary()[org.shortName].LastFilesErrorneous = value;
        else
          bookmarks.KeysAndValues.Add(new DictionaryProxy<string, Bookmark>.KeyAndValue()
          {
            Key = org.shortName,
            Value = new Bookmark() { LastFilesErrorneous = value }
          });
      }
      SetBookmarkLastUpdateDateTime(org, DateTime.UtcNow);
      return this;
    }

    private IBookmarkManager SetBookmarkLastUpdateDateTime(Organization org, DateTime utcNow)
    {
      //Make this thread safe.
      lock (lockContext)
      {
        if (bookmarks.ToDictionary().ContainsKey(org.shortName))
          this.bookmarks.ToDictionary()[org.shortName].LastUpdateDateTime = utcNow;
        else
          bookmarks.KeysAndValues.Add(new DictionaryProxy<string, Bookmark>.KeyAndValue()
          {
            Key = org.shortName,
            Value = new Bookmark() { LastUpdateDateTime = utcNow }
          });
      }
      return this;
    }

    public IBookmarkManager SetBookmarkLastCycleStartDateTime(Organization org, DateTime utcNow)
    {
      //Make this thread safe.
      lock (lockContext)
      {
        if (bookmarks.ToDictionary().ContainsKey(org.shortName))
          this.bookmarks.ToDictionary()[org.shortName].LastCycleStartDateTime = utcNow;
        else
          bookmarks.KeysAndValues.Add(new DictionaryProxy<string, Bookmark>.KeyAndValue()
          {
            Key = org.shortName,
            Value = new Bookmark() { LastCycleStartDateTime = utcNow }
          });
      }
      SetBookmarkLastUpdateDateTime(org, DateTime.UtcNow);
      return this;
    }

    public IBookmarkManager IncBookmarkCyclesCompleted(Organization org)
    {
      //Make this thread safe.
      lock (lockContext)
      {
        if (bookmarks.ToDictionary().ContainsKey(org.shortName))
          this.bookmarks.ToDictionary()[org.shortName].CyclesCompleted++;
        else
          bookmarks.KeysAndValues.Add(new DictionaryProxy<string, Bookmark>.KeyAndValue()
          {
            Key = org.shortName,
            Value = new Bookmark()
          });
      }
      SetBookmarkLastUpdateDateTime(org, DateTime.UtcNow);
      return this;
    }

    public IBookmarkManager SetBookmarkLastCycleStopDateTime(Organization org, DateTime utcNow)
    {
      //Make this thread safe.
      lock (lockContext)
      {
        if (bookmarks.ToDictionary().ContainsKey(org.shortName))
          this.bookmarks.ToDictionary()[org.shortName].LastCycleStopDateTime = utcNow;
        else
          bookmarks.KeysAndValues.Add(new DictionaryProxy<string, Bookmark>.KeyAndValue()
          {
            Key = org.shortName,
            Value = new Bookmark() { LastCycleStopDateTime = utcNow }
          });
      }
      SetBookmarkLastUpdateDateTime(org, DateTime.UtcNow);
      return this;
    }

    public void DeleteInstance()
    {
      self = null;
    }
  }
}
