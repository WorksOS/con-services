using System.Collections.Generic;
using System.Linq;

namespace VSS.Hosted.VLCommon.DataAccess.Views
{
	public class WorkingSetModelDto
	{
		public long AssetId { get; set; }
		public bool HasActiveService { get; set; }
		public bool Selected { get; set; }
	}
	public class DbViewAccess
	{
		private readonly INH_OP _nhOp;
		public DbViewAccess(){}
		public DbViewAccess(INH_OP nhOp)
		{
			_nhOp = nhOp;
		}
		
		/// <summary>
		/// To be used in lieu of querying NH_RPT.vw_WorkingSet in cases where timeouts could occur. 
		/// </summary>
		public List<WorkingSetModelDto> GetWorkingSetItems(long activeUserId)
		{
			return GetAssetWorkingSetItems(activeUserId, true, true);
		}

		/// <summary>
		/// To be used in lieu of querying NH_OP.vw_AssetWorkingSet in cases where timeouts could occur.
		/// Staggers the view queries instead of one giant query.
		/// </summary>
		public List<WorkingSetModelDto> GetAssetWorkingSetItems(long activeUserId, bool filterForActiveService = false, bool filterForSelected = false)
		{
			if (_nhOp == null)
			{
				using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
				{
					return GetAssetWorkingSetItemsCore(opCtx, activeUserId, filterForActiveService, filterForSelected);
				}
			}
			
			return GetAssetWorkingSetItemsCore(_nhOp, activeUserId, filterForActiveService, filterForSelected);
		}

		private List<WorkingSetModelDto> GetAssetWorkingSetItemsCore(INH_OP opCtx, long activeUserId,
			bool filterForActiveService = false, bool filterForSelected = false)
		{
			var subQueries = new List<IQueryable<WorkingSetModelDto>>
				{
					GetWorkingSetActiveUser(activeUserId, opCtx),
					GetWorkingSetImpersonatingUser(activeUserId, opCtx),
					GetWorkingSetImpersonatingDealer(activeUserId, opCtx)
				};

			var ret = new List<WorkingSetModelDto>();

			foreach (var subQ in subQueries)
			{
				ret.AddRange(subQ.Where(x => (!filterForActiveService || x.HasActiveService)
					&& (!filterForSelected || x.Selected)).ToList());
			}

			return ret;
		}


		private IQueryable<WorkingSetModelDto> GetWorkingSetActiveUser(long activeUserId, INH_OP opCtx)
		{
			var ret = (from au in opCtx.ActiveUserReadOnly
				join u in opCtx.UserReadOnly on au.fk_UserID equals u.ID
				join ca in opCtx.CustomerAssetReadOnly on u.fk_CustomerID equals ca.fk_CustomerID
				where !au.Expired && au.fk_ImpersonatedUserID == null
				      && au.ID == activeUserId
				from auasLeft in opCtx.ActiveUserAssetSelectionReadOnly
					.Where(x => au.ID == x.fk_ActiveUserID && x.fk_AssetID == ca.fk_AssetID && !x.Deselected)
					.DefaultIfEmpty()
				select new WorkingSetModelDto
				{
					AssetId = ca.fk_AssetID,
					HasActiveService = ca.HasActiveService,
					Selected = auasLeft != null && auasLeft.fk_ActiveUserID != 0
				});
			return ret;
		}

		private IQueryable<WorkingSetModelDto> GetWorkingSetImpersonatingUser(long activeUserId, INH_OP opCtx)
		{
			var ret = (from au in opCtx.ActiveUserReadOnly
				join u in opCtx.UserReadOnly on au.fk_UserID equals u.ID
				join c in opCtx.CustomerReadOnly on u.fk_CustomerID equals c.ID
				join impersonatedUser in opCtx.UserReadOnly on au.fk_ImpersonatedUserID equals impersonatedUser.ID
				join ca in opCtx.CustomerAssetReadOnly on impersonatedUser.fk_CustomerID equals ca.fk_CustomerID
				where !au.Expired && au.fk_ImpersonatedUserID != null && c.fk_CustomerTypeID == (int)CustomerTypeEnum.Operations
          && au.ID == activeUserId
				from auasLeft in opCtx.ActiveUserAssetSelectionReadOnly
					.Where(x => au.ID == x.fk_ActiveUserID && x.fk_AssetID == ca.fk_AssetID && !x.Deselected)
					.DefaultIfEmpty()
				select new WorkingSetModelDto
				{
					AssetId = ca.fk_AssetID,
					HasActiveService = ca.HasActiveService,
					Selected = auasLeft != null && auasLeft.fk_ActiveUserID != 0
				});
			return ret;
		}

		private IQueryable<WorkingSetModelDto> GetWorkingSetImpersonatingDealer(long activeUserId, INH_OP opCtx)
		{
			var ret = (from au in opCtx.ActiveUserReadOnly
				join impersonator in opCtx.UserReadOnly on au.fk_UserID equals impersonator.ID 
				join impersonated in opCtx.UserReadOnly on au.fk_ImpersonatedUserID equals impersonated.ID
				join torCust in opCtx.CustomerAssetReadOnly on impersonator.fk_CustomerID equals torCust.fk_CustomerID
				from cust in opCtx.CustomerAssetReadOnly
				where impersonated.fk_CustomerID == cust.fk_CustomerID && torCust.fk_AssetID == cust.fk_AssetID
				join dealer in opCtx.CustomerReadOnly  on impersonator.fk_CustomerID equals dealer.ID
				where au.fk_ImpersonatedUserID != null && !au.Expired 
				      && dealer.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer
              && au.ID == activeUserId
				from auasLeft in opCtx.ActiveUserAssetSelectionReadOnly
					.Where(x => au.ID == x.fk_ActiveUserID && x.fk_AssetID == cust.fk_AssetID && !x.Deselected)
					.DefaultIfEmpty()
				select new WorkingSetModelDto
				{
					AssetId = cust.fk_AssetID,
					HasActiveService = cust.HasActiveService,
					Selected = auasLeft != null && auasLeft.fk_ActiveUserID != 0
				});
			return ret;
		}

		
	}
}