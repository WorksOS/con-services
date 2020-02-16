using System;
using System.Linq;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
 public  class IconBuilder
  {
    private int _id = IdGen.GetId();
    private string _description = "GenericNonCAT";

    public IconBuilder Id(int id)
    {
      _id = id;
      return this;
    }

    public IconBuilder Description(string description)
    {
      _description = description;
      return this;
    }
    public Icon Save()
    {
      var icon = Build();

      ContextContainer.Current.OpContext.Icon.AddObject(icon);
      ContextContainer.Current.OpContext.SaveChanges();

      return icon;
    }
    private void CheckValidIcon(int id, string description)
    {
      bool iconExists = ContextContainer.Current.OpContext.IconReadOnly.Any(Icon => Icon.ID == id && Icon.Description == description);
      if (iconExists)
      {
        throw new InvalidOperationException("Can not have multiple Product Families with the same Name and Description.");
      }
    }
    public Icon Build()
    {
      CheckValidIcon(_id, _description);

      var icon = new Icon();
      icon.ID = _id;
      icon.Description = _description;


      return icon;
    }

  }
}
