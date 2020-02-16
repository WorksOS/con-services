using System;
using System.Linq;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder 
{
  public class SalesModelBuilder
  {
    private long _id = IdGen.GetId();
    private string _serialNumberPrefix = "KPZ";
    private string _description = "SalesModel_DESCRIPTION_" + IdGen.GetId();
    private long? _startRange = 1;
    private long? _endRange = 99999;
    private long? _externalID = 7721;
    private DateTime _updateUtc = DateTime.UtcNow;
    private ProductFamily _productFamily = (from pf in ContextContainer.Current.OpContext.ProductFamily where pf.ID == 32 select pf).SingleOrDefault();// = new ProductFamily(){ID=32, Name = "TTT", Description = "Track Type Tractors"};
    private Icon _icon = (from i in ContextContainer.Current.OpContext.Icon where i.ID == 30 select i).SingleOrDefault();
    private string _modelCode = "TEST MODEL";

    public SalesModelBuilder Id(long id)
    {
      _id = id;
      return this;
    }
    public SalesModelBuilder SerialNumberPrefix(string serialNumberPrefix)
    {
      _serialNumberPrefix = serialNumberPrefix;
      return this;
    }
    public SalesModelBuilder Description(string description)
    {
      _description = description;
      return this;
    }  
    public SalesModelBuilder StartRange(long startRange)
    {
      _startRange = startRange;
      return this;
    }
    public SalesModelBuilder EndRange(long endRange)
    {
      _endRange = endRange;
      return this;
    }
    public SalesModelBuilder ExternalID(long? externalID)
    {
      _externalID = externalID;
      return this;
    }
    public SalesModelBuilder UpdateUtc(DateTime updateUtc)
    {
      _updateUtc = updateUtc;
      return this;
    }
    public SalesModelBuilder ForProductFamily(ProductFamily productFamily)
    {
      _productFamily = productFamily;
      return this;
    }
    public SalesModelBuilder ForIcon(Icon icon)
    {
      _icon = icon;
      return this;
    }
    public SalesModelBuilder ModelCode(string modelCode)
    {
      _modelCode = modelCode;
      return this;
    }
    
    public SalesModel Build()
    {
      if (_productFamily == null)
      {
        _productFamily = new ProductFamily(){ID=32, Name = "TTT", Description = "Track Type Tractors"};
      }

      if (_icon == null)
      {
        _icon = new Icon() { ID = 30,  Description = "GenericNonCat" };
      }

      CheckValidSalesModel(_serialNumberPrefix, _startRange, _endRange, _productFamily.ID);
      
      var salesModel = new SalesModel();
      salesModel.ID = _id;
      salesModel.fk_ProductFamilyID = _productFamily.ID;
      salesModel.SerialNumberPrefix = _serialNumberPrefix;
      salesModel.Description = _description;
      salesModel.StartRange = _startRange;
      salesModel.EndRange = _endRange;
      salesModel.UpdateUTC = _updateUtc;
      salesModel.ModelCode = _modelCode;
      salesModel.fk_IconID = _icon.ID;

      return salesModel;
    }
    public SalesModel Save()
    {
      var SalesModel = Build();

      ContextContainer.Current.OpContext.SalesModel.AddObject(SalesModel);
      ContextContainer.Current.OpContext.SaveChanges();

      return SalesModel;
    }

    private void CheckValidSalesModel(string serialNumberPrefix, long? startRange, long? endRange, long familyID)
    {
      bool SalesModelExists = ContextContainer.Current.OpContext.SalesModelReadOnly.Any(SalesModel => SalesModel.SerialNumberPrefix == serialNumberPrefix && SalesModel.StartRange == startRange && SalesModel.EndRange == endRange && SalesModel.fk_ProductFamilyID == familyID);
      if (SalesModelExists)
      {
        throw new InvalidOperationException("Can not have multiple SalesModels with the same SerialNumberPrefix, StartRange, EndRange, and Product Family.");
      }
    }
  }
}
