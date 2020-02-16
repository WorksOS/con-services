using System;
using System.Collections.Generic;
using System.Linq;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common
{
  public class WorkingSetHelper 
  {
    public void Populate(ActiveUser activeUser, bool selected = false)
    {
      if(Helpers.IsMockedContext())
      {
        PopulateMockWorkingSet(activeUser, selected);
      }
      else
      {
        Helpers.ExecuteStoredProcedure(Database.NH_OP, "uspPub_CustomerAsset_Update");

        if(selected)
        {
          var assetIds = ContextContainer.Current.OpContext.vw_AssetWorkingSet.Where(w=>w.fk_ActiveUserID==activeUser.ID).Select(x => x.fk_AssetID).ToList();
          Select(activeUser, assetIds);
        }
      }
    }

    public void Select(ActiveUser activeUser, IList<long> assetIds)
    {
      if (Helpers.IsMockedContext())
      {
        SelectMockWorkingSet(activeUser, assetIds);
      }
      else
      {
        ActiveUserSelectedAssetsAccess.Save(activeUser.ID, assetIds);
      }
    }

    public void Project(ActiveUser activeUser, long projectID, IList<long> assetIds)
    {
      if (Helpers.IsMockedContext())
      {
        ProjectMockWorkingSet(activeUser, projectID, assetIds);
      }
      else
      {
        ActiveUserSelectedAssetsAccess.Save(activeUser.ID, assetIds, projectID);
      }
    } 
  
    private void PopulateMockWorkingSet(ActiveUser activeUser, bool selected)
    {
      var assetsInView = (from asset in ContextContainer.Current.OpContext.Asset
                          join device in ContextContainer.Current.OpContext.DeviceReadOnly on asset.fk_DeviceID equals device.ID
                          join user in ContextContainer.Current.OpContext.UserReadOnly on activeUser.fk_UserID equals user.ID
                          join customer in ContextContainer.Current.OpContext.CustomerReadOnly on user.fk_CustomerID equals customer.ID
                          join serviceView in ContextContainer.Current.OpContext.ServiceView on asset.AssetID equals serviceView.fk_AssetID
                          join service in ContextContainer.Current.OpContext.ServiceReadOnly on serviceView.fk_ServiceID equals service.ID
                          join serviceType in ContextContainer.Current.OpContext.ServiceTypeReadOnly on service.fk_ServiceTypeID equals serviceType.ID
                          let owningCustomer = (ContextContainer.Current.OpContext.CustomerReadOnly.Where(x => x.BSSID == device.OwnerBSSID)).FirstOrDefault()
                          where serviceView.fk_CustomerID == customer.ID && serviceType.IsCore
                          select new
                                 {
                                   Asset = asset,
                                   ServiceTypeId = serviceType.ID,
                                   serviceView.StartKeyDate,
                                   serviceView.EndKeyDate,
                                   ViewingCustomerId = customer.ID,
                                   ViewingCustomerTypeId = customer.fk_CustomerTypeID,
                                   OwningCustomerId = owningCustomer != null ? owningCustomer.ID : 0,
                                   OwningCustomerTypeId = owningCustomer != null ? owningCustomer.fk_CustomerTypeID : 0,
                                 }).ToList();

      foreach (var assetInView in assetsInView) 
      {
        CustomerAsset ca = new CustomerAsset
          {
            fk_AssetID = assetInView.Asset.AssetID,
            fk_CustomerID = assetInView.ViewingCustomerId,
            HasActiveService = (assetInView.StartKeyDate <= DateTime.UtcNow.KeyDate() && assetInView.EndKeyDate >= DateTime.UtcNow.KeyDate()),
            IsOwned = GetOwnership(assetInView.OwningCustomerId, assetInView.OwningCustomerTypeId, assetInView.ViewingCustomerId, assetInView.ViewingCustomerTypeId),
            UpdateUTC = DateTime.UtcNow
          };
        ContextContainer.Current.OpContext.CustomerAsset.AddObject(ca);

        ActiveUserAssetSelection auas = null;
        if (selected)
        {
          auas = new ActiveUserAssetSelection
          {
              fk_ActiveUserID = activeUser.ID,
              fk_AssetID = assetInView.Asset.AssetID
          };
          ContextContainer.Current.OpContext.ActiveUserAssetSelection.AddObject(auas);
        }

        vw_AssetWorkingSet aws = new vw_AssetWorkingSet
        {
           fk_ActiveUserID = activeUser.ID,
           fk_AssetID=ca.fk_AssetID,
           HasActiveService=ca.HasActiveService,
           IsOwned=ca.IsOwned,
           Selected=(null != auas)
        };
        ContextContainer.Current.OpContext.vw_AssetWorkingSet.AddObject(aws);

      }
    }

    private static bool GetOwnership(long owningCustomerId, int owningCustomerTypeId, long viewingCustomerId, int viewingCustomerTypeId) 
    {
      // If Viewing Customer ID equals Owning Customer ID IsOwned = true (probably a dealer or standalone customer)
      if (owningCustomerId == viewingCustomerId)
        return true;

      // If Viewing Customer is EndCustomer and Owning Customer Is Account IsOwned = true
      if (owningCustomerTypeId == (int)CustomerTypeEnum.Account && viewingCustomerTypeId == (int)CustomerTypeEnum.Customer)
        return true;

      // All other combinations return false;
      return false;
    }

    private void SelectMockWorkingSet(ActiveUser activeUser, IList<long> assetIds)
    {
      var workingSetAssets = ContextContainer.Current.OpContext.vw_AssetWorkingSet.Where(x => x.fk_ActiveUserID == activeUser.ID).ToList();

      foreach (var workingSetAsset in workingSetAssets)
      {
        if (assetIds.Contains(workingSetAsset.fk_AssetID))
        {
          var ausa = ContextContainer.Current.OpContext.ActiveUserAssetSelection.Where(x => x.fk_ActiveUserID == activeUser.ID && x.fk_AssetID == workingSetAsset.fk_AssetID).SingleOrDefault();
          if (null == ausa)
          {
            ausa = new ActiveUserAssetSelection { fk_ActiveUserID = activeUser.ID, fk_AssetID = workingSetAsset.fk_AssetID };
            ContextContainer.Current.OpContext.ActiveUserAssetSelection.AddObject(ausa);
          }
        }
      }
    }

    private void ProjectMockWorkingSet(ActiveUser activeUser, long projectID, IList<long> assetIds)
    {
      var workingSetAssets = ContextContainer.Current.OpContext.vw_AssetWorkingSet.Where(x => x.fk_ActiveUserID == activeUser.ID).ToList();

      foreach (var workingSetAsset in workingSetAssets)
      {
        if (assetIds.Contains(workingSetAsset.fk_AssetID))
        {
          var ausa = ContextContainer.Current.OpContext.ActiveUserAssetSelection.Where(x => x.fk_ActiveUserID == activeUser.ID && x.fk_AssetID == workingSetAsset.fk_AssetID && x.fk_ProjectID==projectID).SingleOrDefault();
          if (null == ausa)
          {
            ausa = new ActiveUserAssetSelection { fk_ActiveUserID = activeUser.ID, fk_AssetID = workingSetAsset.fk_AssetID, fk_ProjectID=projectID };
            ContextContainer.Current.OpContext.ActiveUserAssetSelection.AddObject(ausa);
          }
        }
      }
    }
  }
}