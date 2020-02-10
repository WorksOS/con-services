using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Enums;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace VSS.MasterData.WebAPI.AssetRepository
{
	public class SupportAssetServices :  ISupportAssetServices
	{
		private readonly ITransactions _transaction;
		private readonly ILogger _logger;
		
		public SupportAssetServices( ITransactions transactions, ILogger logger)
		{
			_transaction = transactions;
			_logger = logger;
		}

		public List<AssetCustomer> GetAssetCustomerByAssetGuid(Guid assetUid)
		{
			string getCustomersQuery = string.Format(@"SELECT 
											ac.fk_CustomerUID AS CustomerUID,
											c.CustomerName AS CustomerName,
											IFNULL(c.fk_CustomerTypeID,-1) AS CustomerType,
											c1.CustomerUID AS ParentCustomerUID,
											c1.CustomerName AS ParentName,
											IFNULL(c1.fk_CustomerTypeID,-1) AS ParentCustomerType
										FROM
											md_customer_CustomerAsset ac
												LEFT OUTER JOIN
											md_customer_Customer c ON c.CustomerUID = ac.fk_CustomerUID
												LEFT OUTER JOIN
											md_customer_CustomerRelationshipNode crn ON crn.fk_CustomerUID = c.CustomerUID
												LEFT OUTER JOIN
											md_customer_Customer c1 ON c1.CustomerUID = crn.fk_ParentCustomerUID
												AND c1.CustomerUID IN (SELECT 
													acs.fk_CustomerUID
												FROM
													md_customer_CustomerAsset acs
												WHERE
													acs.fk_AssetUID = ac.fk_AssetUID)
										WHERE
											ac.fk_AssetUID = {0};", assetUid.ToStringWithoutHyphens().WrapWithUnhex());
			var assetData= _transaction.Get<AssetCustomer>((getCustomersQuery))?.ToList();
			var assetCustomer = assetData?.Select(c => new AssetCustomer
			  {
				  CustomerName = c.CustomerName,
				  CustomerUID = c.CustomerUID,
				  CustomerType = (c.CustomerType != "-1") ? ((CustomerType)Enum.Parse(typeof(CustomerType), c.CustomerType)).ToString() : null,
				  ParentCustomerUID = c.ParentCustomerUID ?? null,
				  ParentName = c.ParentCustomerUID.HasValue ? c.ParentName : null,
				  ParentCustomerType = c.ParentCustomerUID.HasValue && (c.CustomerType != "-1") ? ((CustomerType)Enum.Parse(typeof(CustomerType), c.CustomerType)).ToString() : null,
			  }).ToList();
			return assetCustomer;
		}


		public List<ClientModel.Customer> GetCustomerByCustomerGuids(Guid[] customerGUIDs)
		{
			string getCustomerQuery = $"SELECT CustomerUID,CustomerID,CustomerName,fk_CustomerTypeID,LastCustomerUTC" +
				$" FROM md_customer_Customer WHERE CustomerUID in ({0});";
			var customerGuids = customerGUIDs.Select(g => g.ToStringAndWrapWithUnhex()).ToList();
			var customersData = _transaction.Get<DbCustomer>(string.Format(getCustomerQuery, string.Join(",", customerGuids)))?.ToList();
			var customers = customersData?.Select(c => new ClientModel.Customer
			{
				CustomerName = c.CustomerName,
				CustomerUID = new Guid(c.CustomerUID.ToString()),
				CustomerType = ((CustomerType)Enum.ToObject(typeof(CustomerType), c.fk_CustomerTypeID)).ToString()
			}).ToList();
			return customers;
		}

		public AssetSubscriptionModel GetSubscriptionForAsset(Guid assetGuid)
		{
			string getAssetSubscriptionQuery = "Select asn.AssetSubscriptionUID as SubscriptionUID,asn.StartDate as SubscriptionStartDate,asn.EndDate as SubscriptionEndDate,asn.fk_CustomerUID as CustomerUID, " +
												"(case UTC_TIMESTAMP() between asn.StartDate and asn.EndDate when true then 'Active' else 'InActive' end)  as SubscriptionStatus, st.Name as SubscriptionName " +
												"FROM md_subscription_AssetSubscription asn " +
												"join md_subscription_ServiceType st on st.ServiceTypeID = asn.fk_ServiceTypeID and st.fk_ServiceTypeFamilyID=1 where asn.fk_AssetUID= " + assetGuid.ToStringAndWrapWithUnhex() + "; ";
			List<OwnerVisibility> assetSubscriptonList = _transaction.Get<OwnerVisibility>(getAssetSubscriptionQuery).ToList();
			AssetSubscriptionModel assetSubscription = new AssetSubscriptionModel();
			if (assetSubscriptonList.Any() && assetSubscriptonList[0] != null)
			{
				var lstCustomers = GetCustomerByCustomerGuids(assetSubscriptonList.Select(x => x.CustomerUID).Distinct().ToList().ToArray());
				if (lstCustomers.Any())
				{
					foreach (OwnerVisibility vi in assetSubscriptonList)
					{
						var customer = lstCustomers.Where(x => x.CustomerUID == vi.CustomerUID).Select(y => y).ToList().FirstOrDefault();
						if (customer != null)
						{
							vi.CustomerName = customer.CustomerName;
							vi.CustomerType = customer.CustomerType;
						}
					}
				}
				assetSubscription.AssetUID = assetGuid;
				assetSubscription.SubscriptionStatus = (assetSubscriptonList.Where(s => s.SubscriptionStatus == "Active").ToList().Any()) == true ? "Active" : "InActive";
				assetSubscription.OwnersVisibility = assetSubscriptonList.ToList();
			}
			return assetSubscription;
		}

		public List<AssetDetail> GetAssetDetailFromAssetGuids(List<Guid> assetUIDs)
		{
			var assets = new List<AssetDetail>();
			var query = "";
			List<string> guidStrings;
			if (assetUIDs != null)
			{
				guidStrings = assetUIDs.Select(assetUID => ((Guid)assetUID).ToStringWithoutHyphens().WrapWithUnhex()).ToList();				
				query = $"SELECT hex(a.AssetUID) as AssetUID,a.AssetName,a.SerialNumber,a.MakeCode,a.Model," +
				$" a.AssetTypeName,a.ModelYear,hex(a.OwningCustomerUID),a.UpdateUTC as TimestampOfModification,d.SerialNumber as DeviceSerialNumber," +
				$" dt.TypeName as DeviceType,d.fk_DeviceStatusID as DeviceState,hex(d.DeviceUID) as DeviceUID ,group_concat(hex(ac.fk_CustomerUID) separator ',')  as AssetCustomerUID FROM " +
				$" md_asset_Asset a " +
				$" left outer join md_customer_CustomerAsset ac on ac.fk_AssetUID = a.AssetUID " +
				$" left outer join md_asset_AssetDevice ad on ad.fk_AssetUID = a.AssetUID " +
				$" left outer join md_device_Device d on d.DeviceUID = ad.fk_DeviceUID " +
				$" Join md_device_DeviceType dt on dt.DeviceTypeID = d.fk_DeviceTypeID" +
				$" where a.StatusInd=1 and a.AssetUID in ({string.Join(", ", guidStrings)}) group by a.AssetUID";
			}
			try
			{
				assets.AddRange(_transaction.Get <AssetDetail>(query).ToList());
			}
			catch (Exception ex)
			{
				_logger.LogError("GetAssetDetailFromAssetGuids Called: " + ex.Message);
			}
			 
			return assets;
		}

		
	}
}

 