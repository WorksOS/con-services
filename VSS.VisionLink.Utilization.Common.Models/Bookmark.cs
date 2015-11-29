using VSS.VisionLink.Landfill.Common.Models;

namespace VSS.VisionLink.Utilization.Common.Models
{
  public class Bookmark
  {
    public BookmarkTypeEnum BookmarkType { get; set; }
    public long Value { get; set; }

    public override bool Equals(object obj)
    {
      var otherBookmark = obj as Bookmark;
      return otherBookmark != null && (otherBookmark.BookmarkType == BookmarkType
                                       && otherBookmark.Value == Value);
    }
    public override int GetHashCode() { return 0; }
  }
}