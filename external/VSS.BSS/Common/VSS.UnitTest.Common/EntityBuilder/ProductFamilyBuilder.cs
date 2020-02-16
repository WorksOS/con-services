using System;
using System.Linq;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder 
{
  public class ProductFamilyBuilder
  {
    private long _id = IdGen.GetId();
    private string _name = "ProductFamily_Name" + IdGen.GetId();
    private string _description = "ProductFamily_DESCRIPTION_" + IdGen.GetId();
    private DateTime _updateUtc = DateTime.UtcNow;

    public ProductFamilyBuilder Id(long id)
    {
      _id = id;
      return this;
    }

    public ProductFamilyBuilder Name(string name)
    {
      _name = name;
      return this;
    }

    public ProductFamilyBuilder Description(string description)
    {
      _description = description;
      return this;
    }
    
    public ProductFamily Build()
    {
      CheckValidProductFamily(_name, _description);
      
      var productFamily = new ProductFamily();
      productFamily.ID = _id;
      productFamily.Name = _name;
      productFamily.Description = _description;
      productFamily.UpdateUTC = _updateUtc;

      return productFamily;
    }
    public ProductFamily Save()
    {
      var productFamily = Build();

      ContextContainer.Current.OpContext.ProductFamily.AddObject(productFamily);
      ContextContainer.Current.OpContext.SaveChanges();

      return productFamily;
    }

    private void CheckValidProductFamily(string name, string description)
    {
      bool productFamilyExists = ContextContainer.Current.OpContext.ProductFamilyReadOnly.Any(ProductFamily => ProductFamily.Name == name && ProductFamily.Description == description);
      if (productFamilyExists)
      {
        throw new InvalidOperationException("Can not have multiple Product Families with the same Name and Description.");
      }
    }
  }
}
