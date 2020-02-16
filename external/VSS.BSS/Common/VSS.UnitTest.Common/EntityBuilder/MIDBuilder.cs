using VSS.UnitTest.Common.Contexts;
using VSS.Hosted.VLCommon;


namespace VSS.UnitTest.Common.EntityBuilder
{
  public class MIDBuilder
  {
    private long _id = IdGen.GetId();
    private string _midID;

    public virtual MIDBuilder MIDID(string midID)
    {
      _midID = midID;
      return this;
    }

    public MID Build()
    {
      MID mid = new MID();
      mid.ID = _id;
      mid.MID1 = _midID;

      return mid;
    }

    public virtual MID Save()
    {
      MID mid = Build();

      ContextContainer.Current.OpContext.MID.AddObject(mid);
      ContextContainer.Current.OpContext.SaveChanges();

      return mid;
    }
  }
}
