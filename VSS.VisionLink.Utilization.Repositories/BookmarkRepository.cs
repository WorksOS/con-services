using System.Linq;
using System.Threading.Tasks;
using Dapper;
using VSS.VisionLink.Utilization.Common.Interfaces;
using VSS.VisionLink.Utilization.Common.Models;

namespace VSS.VisionLink.Utilization.Repositories
{
  public class BookmarkRepository : RepositoryBase, IBookmarkRepository
  {
    public BookmarkRepository(string connectionString)
      : base(connectionString)
    {
    }

    public async Task<int> SaveBookmark(Bookmark bookmark)
    {
      PerhapsOpenConnection();
      var upsertedCount = 0;

      var existing = (await Connection.QueryAsync<Bookmark>
        (@"SELECT fk_BookmarkTypeID AS BookmarkType, Value
              FROM Bookmark
              WHERE fk_BookmarkTypeID = @bType"
          , new {bType = (int) bookmark.BookmarkType}
        )).FirstOrDefault();

      if (existing == null)
      {
        const string insert =
          @"INSERT Bookmark
                (fk_BookmarkTypeID, Value)
              VALUES
                (@BookmarkType, @Value)";
        upsertedCount = await Connection.ExecuteAsync(insert, bookmark);
      }
      else
      {
        const string update =
          @"UPDATE Bookmark                
                SET Value = @value
              WHERE fk_BookmarkTypeID = @bType";
        upsertedCount = await Connection.ExecuteAsync(update,
          new {value = bookmark.Value, bType = (int) bookmark.BookmarkType});
      }
      PerhapsCloseConnection();
      return upsertedCount;
    }

    public Bookmark GetBookmark(BookmarkTypeEnum bookmarkType)
    {
      PerhapsOpenConnection();
      var bookmark = Connection.Query<Bookmark>
        (@"SELECT fk_BookmarkTypeID AS BookmarkType, Value
              FROM Bookmark
              WHERE fk_BookmarkTypeID = @bType"
          , new {bType = (int) bookmarkType}
        ).FirstOrDefault();
      PerhapsCloseConnection();
      return bookmark;
    }
  }
}