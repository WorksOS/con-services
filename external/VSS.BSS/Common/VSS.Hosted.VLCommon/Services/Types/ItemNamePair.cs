using System;

namespace VSS.Hosted.VLCommon
{  
  public class ItemNamePair : IComparable<ItemNamePair>
  {
    public long ID;
    public string name;

    public int CompareTo(ItemNamePair that)
    {
      return name.CompareTo(that.name);
    }
  }
}
