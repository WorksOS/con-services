using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace VSS.Hosted.VLCommon
{
  public static class EntityExtensions
  {
    public static IQueryable<int> AssetDeviceICDSeries(this INH_OP opCtx, long assetID)
    {
      return opCtx.AssetReadOnly.Where(w => w.AssetID == assetID)
          .Join(opCtx.DeviceReadOnly, outerKey => outerKey.fk_DeviceID, innerKey => innerKey.ID, (left, me) => me.fk_DeviceTypeID)
          .Join(opCtx.DevicePartNumberReadOnly, outerKey => outerKey, innerKey => innerKey.fk_DeviceTypeID, (left, me) => (int)me.fk_DeviceICDSeriesID);
    }

    public static bool IsICDSeries(this INH_OP opCtx, long assetID, DeviceICDSeriesEnum series)
    {
      return opCtx.AssetDeviceICDSeries(assetID).Where(w => w == (int)series).Select(s => 1).Count() == 1;
    }

    public static IQueryable<int> ServiceTypeFeatures(this INH_OP opCtx, long customerID, long assetID, int? fromKeyDate = null, int? toKeyDate = null)
    {
      int utcNowKeyDate = DateTime.UtcNow.KeyDate();
      int from = fromKeyDate ?? utcNowKeyDate;
      int to = toKeyDate ?? utcNowKeyDate;

      return opCtx.CustomerAssetReadOnly.Where(w => w.fk_CustomerID == customerID && w.fk_AssetID == assetID)
        .Join(opCtx.ServiceViewReadOnly, outerKey => new { outerKey.fk_CustomerID, outerKey.fk_AssetID },
                                        innerKey => new { innerKey.fk_CustomerID, innerKey.fk_AssetID },
                                        (left, me) => new { CA = left, SV = me }).Where(w => w.SV.StartKeyDate <= from
                                                                                             && w.SV.EndKeyDate >= to)
        .Join(opCtx.ServiceReadOnly, outerKey => outerKey.SV.fk_ServiceID, innerKey => innerKey.ID, (left, me) => new { CA = left.CA, S = me })
        .Join(opCtx.ServiceTypeAppFeatureReadOnly, outerKey => outerKey.S.fk_ServiceTypeID, innerKey => innerKey.fk_ServiceTypeID,
                                        (left, me) => new { CA = left.CA, STAF = me })
        .Join(opCtx.AppFeatureReadOnly, outerKey => outerKey.STAF.fk_AppFeatureID, innerKey => innerKey.ID, (left, me) => new { CA = left.CA, AF = me })
        .Where(w => w.CA.IsOwned || false == w.AF.IsOwnershipRequired)
        .Select(s => s.AF.ID);
    }

		
    private static IQueryable<int> DeviceTypeFeatures(this INH_OP opCtx, long customerID, long assetID)
    {
      return opCtx.CustomerAssetReadOnly.Where(w => w.fk_CustomerID == customerID && w.fk_AssetID == assetID)
        .Join(opCtx.AssetReadOnly, outerKey => outerKey.fk_AssetID, innerKey => innerKey.AssetID, (left, me) => new { CA = left, A = me })
        .Join(opCtx.DeviceReadOnly, outerKey => outerKey.A.fk_DeviceID, innerKey => innerKey.ID, (left, me) => new { CA = left.CA, D = me })
        .Join(opCtx.DeviceTypeReadOnly, outerKey => outerKey.D.fk_DeviceTypeID, innerKey => innerKey.ID, (left, me) => new { CA = left.CA, DT = me })
        .Join(opCtx.AppFeatureSetAppFeatureReadOnly, outerKey => outerKey.DT.fk_AppFeatureSetID, innerKey => innerKey.fk_AppFeatureSetID,(left,me) => new { CA=left.CA, AFSAF = me})
        .Join(opCtx.AppFeatureReadOnly,  outerKey => outerKey.AFSAF.fk_AppFeatureID, innerKey => innerKey.ID,(left,me) => new { CA= left.CA, AF = me})
        .Where(w => w.CA.IsOwned || false == w.AF.IsOwnershipRequired)
        .Select(s => s.AF.ID);
    }

    public static IQueryable<int> Features(this INH_OP opCtx, long customerID, long assetID, int? fromKeyDate = null, int? toKeyDate = null)
    {
      return opCtx.ServiceTypeFeatures(customerID, assetID, fromKeyDate, toKeyDate).Intersect(opCtx.DeviceTypeFeatures(customerID, assetID));
    }

    public static bool HasFeature(this INH_OP opCtx, long customerID, long assetID, AppFeatureEnum feature, int? fromKeyDate = null, int? toKeyDate = null)
    {
      return opCtx.Features(customerID, assetID, fromKeyDate, toKeyDate).Any(f => f == (int)feature);
    }

		/// <summary>
		/// Obsolete. Use the AppFeatureMap class instead.
		/// </summary>
		[Obsolete]
    public static bool HasDeviceFeature(this INH_OP opCtx, long customerID, long assetID, AppFeatureEnum feature)
    {
      return opCtx.DeviceTypeFeatures(customerID, assetID).Any(f => f == (int)feature);
    }

    public static IQueryable<Asset> GetActiveAssetsForCustomer(this INH_OP ctx, long customerId)
    {
      return (from ca in ctx.CustomerAssetReadOnly
              join a in ctx.AssetReadOnly on ca.fk_AssetID equals a.AssetID
              where ca.fk_CustomerID == customerId &&
              ca.HasActiveService
              select a);
    }
  }
}
