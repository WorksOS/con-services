using System;
using System.Linq;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder 
{
  public class PMSalesModelBuilder
  {
    private long _id = IdGen.GetId();
    private string _serialNumberPrefix = "KPZ";
    private int? _startRange = 1;
    private int? _endRange = 99999;
    private long? _externalID = null;
    private DateTime _updateUtc = DateTime.UtcNow;
    private string _makeCode = "CAT";
    private string _model = null;
    
    public PMSalesModelBuilder Id(long id)
    {
      _id = id;
      return this;
    }
    public PMSalesModelBuilder SerialNumberPrefix(string serialNumberPrefix)
    {
      _serialNumberPrefix = serialNumberPrefix;
      return this;
    }
    public PMSalesModelBuilder StartRange(int startRange)
    {
      _startRange = startRange;
      return this;
    }
    public PMSalesModelBuilder EndRange(int endRange)
    {
      _endRange = endRange;
      return this;
    }
    public PMSalesModelBuilder ExternalID(long? externalID)
    {
      _externalID = externalID;
      return this;
    }
    public PMSalesModelBuilder UpdateUtc(DateTime updateUtc)
    {
      _updateUtc = updateUtc;
      return this;
    }
    public PMSalesModelBuilder MakeModel(string make, string model)
    {
      this._makeCode = make;
      this._model = model;
      return this;
    }
    public PMSalesModel Build()
    {
      CheckValidPMSalesModel(_serialNumberPrefix, _startRange, _endRange, _makeCode, _model);
      
      var salesModel = new PMSalesModel();
      salesModel.ID = _id;
      salesModel.SerialNumberPrefix = _serialNumberPrefix;
      salesModel.StartRange = _startRange;
      salesModel.EndRange = _endRange;
      salesModel.ExternalID = _externalID;
      salesModel.fk_MakeCode = _makeCode;
      salesModel.Model = _model;
      salesModel.UpdateUTC = _updateUtc;
      
      return salesModel;
    }
    public PMSalesModel Save()
    {
      var PMSalesModel = Build();

      ContextContainer.Current.OpContext.PMSalesModel.AddObject(PMSalesModel);
      ContextContainer.Current.OpContext.SaveChanges();

      return PMSalesModel;
    }

    private void CheckValidPMSalesModel(string serialNumberPrefix, int? startRange, int? endRange, string makeCode, string model)
    {
      bool PMSalesModelExists = (ContextContainer.Current.OpContext.PMSalesModelReadOnly.Any(
        PMSalesModel => PMSalesModel.SerialNumberPrefix == serialNumberPrefix 
          && PMSalesModel.StartRange == startRange && PMSalesModel.EndRange == endRange
          && PMSalesModel.fk_MakeCode == makeCode
          && PMSalesModel.Model == model));
      if (PMSalesModelExists)
      {
        throw new InvalidOperationException("Can not have multiple PMSalesModels with the same SerialNumberPrefix, StartRange, EndRange, or Make/Model.");
      }
    }
  }
}
