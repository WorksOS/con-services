using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common.Contexts;

namespace VSS.UnitTest.Common.EntityBuilder
{
 public class AppFeatureSetAppFeatureBuilder
  {
    #region Defaults

    private int _id = IdGen.GetId();
    private AppFeatureEnum _appFeatureId = AppFeatureEnum.AssetSecurityApi;
    private AppFeatureSetEnum _appFeatureSetId = AppFeatureSetEnum.Series522;
    private bool _IsOwnershipRequired = false;

    #endregion

    public virtual AppFeatureSetAppFeatureBuilder AppFeatureId(AppFeatureEnum appFeatureId)
    {
      _appFeatureId = appFeatureId;
      return this;
    }
    public virtual AppFeatureSetAppFeatureBuilder AppFeatureSetId(AppFeatureSetEnum AppFeatureSetId)
    {
      _appFeatureSetId = AppFeatureSetId;
      return this;
    }

    public AppFeatureSetAppFeature Build()
    {
      

      AppFeatureSetAppFeature appFSAppF = new AppFeatureSetAppFeature();

      appFSAppF.ID = _id;
      appFSAppF.fk_AppFeatureID = (int)_appFeatureId;
      appFSAppF.fk_AppFeatureSetID = (int)_appFeatureSetId;
      appFSAppF.IsOwnershipRequired = _IsOwnershipRequired;      

      return appFSAppF;
    }
    public virtual AppFeatureSetAppFeature Save()
    {
      AppFeatureSetAppFeature appFSAppF = Build();

      ContextContainer.Current.OpContext.AppFeatureSetAppFeature.AddObject(appFSAppF);
      ContextContainer.Current.OpContext.SaveChanges();

      return appFSAppF;
    }
  }
}
