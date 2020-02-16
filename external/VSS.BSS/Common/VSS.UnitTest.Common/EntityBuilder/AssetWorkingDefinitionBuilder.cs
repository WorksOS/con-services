using System;
using System.Linq;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class AssetWorkingDefinitionBuilder 
  {
    #region AssetWorkingDefinition Fields

    private long _assetId;
    private Asset _asset;
    private WorkDefinitionEnum _workDefinition = WorkDefinitionEnum.MeterDelta;
    private int _sensorNumber = 0;
    private bool _sensorStartIsOn = true;
    private DateTime _updateUtc = DateTime.UtcNow;
    

    #endregion

    public virtual AssetWorkingDefinitionBuilder WorkDefinition(WorkDefinitionEnum workingDefinition)
    {
      _workDefinition = workingDefinition;
      return this;
    }
    public virtual AssetWorkingDefinitionBuilder SensorNumber(int sensorNumber)
    {
      _sensorNumber = sensorNumber;
      return this;
    }
    public virtual AssetWorkingDefinitionBuilder SensorStartIsOn(bool sensorStartIsOn)
    {
      _sensorStartIsOn = sensorStartIsOn;
      return this;
    }
    public virtual AssetWorkingDefinitionBuilder UpdateUtc(DateTime updateUtc)
    {
      _updateUtc = updateUtc;
      return this;
    }
    public virtual AssetWorkingDefinitionBuilder ForAsset(long assetId)
    {
      _assetId = assetId;
      return this;
    }
    public virtual AssetWorkingDefinitionBuilder ForAsset(Asset asset)
    {
      _asset = asset;
      _assetId = asset.AssetID;
      return this;
    }
    public virtual AssetWorkingDefinitionBuilder SyncWithRpt()
    {
      return this;
    }

    private AssetWorkingDefinition Build() 
    {
      var assetWorkingDefinition = new AssetWorkingDefinition();

      assetWorkingDefinition.fk_WorkDefinitionID = (int)_workDefinition;
      assetWorkingDefinition.SensorNumber = _sensorNumber;
      assetWorkingDefinition.SensorStartIsOn = _sensorStartIsOn;
      assetWorkingDefinition.UpdateUTC = _updateUtc;

      if (_asset != null)
        assetWorkingDefinition.fk_AssetID = _asset.AssetID;
      else
        assetWorkingDefinition.fk_AssetID = _assetId;

      return assetWorkingDefinition;
    }
    
    public virtual AssetWorkingDefinition Save() 
    {
      AssetWorkingDefinition assetWorkingDefinitionBuilt = Build();
      AssetWorkingDefinition assetWorkingDefinition = (from awd in ContextContainer.Current.OpContext.AssetWorkingDefinition
                                                              where awd.fk_AssetID == _assetId
                                                              select awd).FirstOrDefault();
      if (assetWorkingDefinition != null)
      {
        assetWorkingDefinition.fk_WorkDefinitionID = assetWorkingDefinitionBuilt.fk_WorkDefinitionID;
        assetWorkingDefinition.SensorNumber = assetWorkingDefinitionBuilt.SensorNumber;
        assetWorkingDefinition.SensorStartIsOn = assetWorkingDefinitionBuilt.SensorStartIsOn;
        assetWorkingDefinition.UpdateUTC = assetWorkingDefinitionBuilt.UpdateUTC;
      }
      else
      {
        assetWorkingDefinition = assetWorkingDefinitionBuilt;
        ContextContainer.Current.OpContext.AssetWorkingDefinition.AddObject(assetWorkingDefinition);
      }
      
      ContextContainer.Current.OpContext.SaveChanges();
      return assetWorkingDefinition;
    }
  }
}
