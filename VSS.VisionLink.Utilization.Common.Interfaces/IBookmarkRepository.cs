using System.Threading.Tasks;
using VSS.VisionLink.Utilization.Common.Models;

namespace VSS.VisionLink.Utilization.Common.Interfaces
{
  public interface IBookmarkRepository
  {
    Bookmark GetBookmark(BookmarkTypeEnum bookmarkType);
    Task<int> SaveBookmark(Bookmark bookmark);
  }
}