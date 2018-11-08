using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileHarvester.Models;

namespace VSS.Productivity3D.TagFileHarvester.Interfaces
{
  public interface IBookmarkManager
  {
    Bookmark GetBookmark(Organization org);
    void UpdateBookmark(Organization org, Bookmark bookmark);
    Task WriteBookmarksAsync();
    IBookmarkManager WriteBookmarks();
    IBookmarkManager SetBookmarkUTC(Organization org, DateTime utcNow);
    IBookmarkManager SetBookmarkLastCycleStartDateTime(Organization org, DateTime utcNow);
    IBookmarkManager SetBookmarkLastCycleStopDateTime(Organization org, DateTime utcNow);
    IBookmarkManager SetBookmarkInProgress(Organization org, bool value);
    IBookmarkManager SetBookmarkLastFilesProcessed(Organization org, int value);
    IBookmarkManager SetBookmarkLastFilesRefused(Organization org, int value);
    IBookmarkManager SetBookmarkLastFilesError(Organization org, int value);
    int MergeWithUpdatedBookmarks();
    void StartDataExport();
    IBookmarkManager IncBookmarkCyclesCompleted(Organization org);
    void StopDataExport();
    IBookmarkManager SetBookmarkLastTCCScanTimeUTC(Organization org, DateTime utcNow);
  }
}
