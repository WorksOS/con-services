using System.Threading.Tasks;
using VSS.VisionLink.Landfill.Common.Models;

namespace VSS.VisionLink.Landfill.Common.Interfaces
{
  public interface IBookmarkRepository
  {
    Bookmark GetBookmark(BookmarkTypeEnum bookmarkType);
    Task<int> SaveBookmark(Bookmark bookmark);
  }
}