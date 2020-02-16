using System;
using System.Linq;
using VSS.Hosted.VLCommon.Events;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ActivityHelper
  {
    public static CustomerCreatedEvent GetCustomerCreatedMessage(CustomerContext context)
    {
      var customer = API.Customer.GetCustomer(context.Id);

      bool hasParentDealer = context.NewParent.Exists && context.NewParent.RelationshipType == CustomerRelationshipTypeEnum.TCSDealer;
      bool hasParentCustomer = context.NewParent.Exists && context.NewParent.RelationshipType == CustomerRelationshipTypeEnum.TCSCustomer;

      return new CustomerCreatedEvent
                                    {
                                      Source = (int)EventSourceEnum.NhBss,
                                      CreatedUtc = DateTime.UtcNow,
                                      CustomerId = context.Id,
                                      CustomerType = context.New.Type.ToString(),
                                      BssId = context.New.BssId,
                                      Name = context.New.Name,
                                      CustomerGuid = (customer != null) ? ((customer.CustomerUID != null) ? customer.CustomerUID.ToString().ToLower() : string.Empty) : string.Empty,

                                      ParentDealerId =  hasParentDealer ? context.NewParent.Id : 0,
                                      ParentDealerName = hasParentDealer ? context.NewParent.Name : string.Empty,
                                      ParentDealerGuid = hasParentDealer ? ((context.NewParent.CustomerUId != null) ? context.NewParent.CustomerUId.ToString().ToLower() : string.Empty) : string.Empty,

                                      ParentCustomerId = hasParentCustomer ? context.NewParent.Id : 0,
                                      ParentCustomerName = hasParentCustomer ? context.NewParent.Name : string.Empty,
                                      ParentCustomerGuid = hasParentCustomer ? ((context.NewParent.CustomerUId != null) ? context.NewParent.CustomerUId.ToString().ToLower() : string.Empty) : string.Empty
                                      // ParentId = (context.NewParent.Exists) ? context.NewParent.Id : 0
                                    };
    }

    public static CustomerUpdatedEvent GetCustomerUpdatedMessage(CustomerContext context)
    {
      return new CustomerUpdatedEvent
                                    {
                                      Source = (int) EventSourceEnum.NhBss,
                                      CreatedUtc = DateTime.UtcNow,
                                      CustomerId = context.Id,
                                      CustomerType = context.New.Type.ToString(),
                                      BssId = context.New.BssId,
                                      Name = context.New.Name,
                                      CustomerGuid = context.CustomerUId.ToString().ToLower(),

                                      ParentDealerId = context.ParentDealer.Exists ? context.ParentDealer.Id : 0,
                                      ParentDealerName = context.ParentDealer.Exists ? context.ParentDealer.Name : string.Empty,
                                      ParentDealerGuid = context.ParentDealer.Exists ? ((context.ParentDealer.CustomerUId != null) ? context.ParentDealer.CustomerUId.ToString().ToLower() : string.Empty) : string.Empty,

                                      ParentCustomerId = context.ParentCustomer.Exists ? context.ParentCustomer.Id : 0,
                                      ParentCustomerName = context.ParentCustomer.Exists ? context.ParentCustomer.Name : string.Empty,
                                      ParentCustomerGuid = context.ParentCustomer.Exists ? ((context.ParentCustomer.CustomerUId != null) ? context.ParentCustomer.CustomerUId.ToString().ToLower() : string.Empty) : string.Empty
                                    };
    }

    public static AssetCreatedEvent GetAssetCreatedMessage(AssetDeviceContext context)
    {
      var parentCustomerInfo = GetParentCustomerInfo(context);

      return new AssetCreatedEvent
                                 {
                                   Source = (int)EventSourceEnum.NhBss,
                                   CreatedUtc = DateTime.UtcNow,
                                   ProductFamilyName = GetProductFamily(context),
                                   Model = context.Asset.Model,
                                   ManufactureYear = context.Asset.ManufactureYear,
                                   MakeCode = context.Asset.MakeCode,
                                   SerialNumber = context.Asset.SerialNumber,
                                   AssetId = context.Asset.AssetId,
                                   OwnerId = context.Device.OwnerId,
                                   Name = context.Asset.Name,
                                   ParentCustomerId = parentCustomerInfo.Item1,    // parentCustomerId
                                   ParentCustomerName = parentCustomerInfo.Item2   // parentCustomerName
                                 };
    }

    public static AssetUpdatedEvent GetAssetUpdatedMessage(AssetDeviceContext context)
    {
      var parentCustomerInfo = GetParentCustomerInfo(context);
      
      return new AssetUpdatedEvent
                                 {
                                   Source = (int) EventSourceEnum.NhBss,
                                   CreatedUtc = DateTime.UtcNow,
                                   ProductFamilyName = GetProductFamily(context),
                                   Model = context.Asset.Model,
                                   ManufactureYear = context.Asset.ManufactureYear,
                                   MakeCode = context.Asset.MakeCode,
                                   SerialNumber = context.Asset.SerialNumber,
                                   AssetId = context.Asset.AssetId,
                                   OwnerId = context.Device.OwnerId,
                                   Name = context.Asset.Name,
                                   ParentCustomerId = parentCustomerInfo.Item1,    // parentCustomerId
                                   ParentCustomerName = parentCustomerInfo.Item2   // parentCustomerName
                                 };
    }

    public static DeviceOwnershipTransferredEvent GetDeviceOwnershipTransferredMessage(AssetDeviceContext context, long oldOwnerId)
    {
      var parentCustomerInfo = GetParentCustomerInfo(context);

      return new DeviceOwnershipTransferredEvent
                                                {
                                                  Source = (int)EventSourceEnum.NhBss,
                                                  CreatedUtc = DateTime.UtcNow,
                                                  NewCustomerId = context.Device.OwnerId,
                                                  CustomerId = oldOwnerId,
                                                  AssetId = context.Asset.AssetId,
                                                  SerialNumber = context.Asset.SerialNumber,
                                                  ParentCustomerId = parentCustomerInfo.Item1,    // parentCustomerId
                                                  ParentCustomerName = parentCustomerInfo.Item2   // parentCustomerName
                                                };
    }

    public static long GetAssetId(string ibKey)
    {
      return (from a in Data.Context.OP.AssetReadOnly
              join d in Data.Context.OP.DeviceReadOnly on a.fk_DeviceID equals d.ID
              where d.IBKey == ibKey
              select a.AssetID).FirstOrDefault();
    }

    private static string GetProductFamily(AssetDeviceContext context)
    {
      var productFamily = (from a in Data.Context.OP.AssetReadOnly
                           where a.AssetID == context.Asset.AssetId
                           select a.ProductFamilyName).FirstOrDefault();

      return string.IsNullOrWhiteSpace(productFamily) ? null : productFamily;
    }

    private static Tuple<long, string> GetParentCustomerInfo(AssetDeviceContext context)
    {
      long parentCustomerId = 0;
      string parentCustomerName = "";

      if (context.Device.Owner != null)
      {
        if (context.Device.Owner.Type == CustomerTypeEnum.Dealer)
        {
          parentCustomerId = context.Device.OwnerId;
          parentCustomerName = context.Device.Owner.Name;
        }
        else if (context.Device.Owner.Type == CustomerTypeEnum.Account)
        {
          var parentCustomer = Services.Customers().GetParentCustomerByChildCustomerId(context.Device.OwnerId).Item1;
          if (parentCustomer != null)
          {
            parentCustomerId = parentCustomer.ID;
            parentCustomerName = parentCustomer.Name;
          }
          else // An account without customer but with dealer only is not a valid scenario, but still!
          {
            var parentDealer = Services.Customers().GetParentDealerByChildCustomerId(context.Device.OwnerId).Item1;
            if (parentDealer != null)
            {
              parentCustomerId = parentDealer.ID;
              parentCustomerName = parentDealer.Name;
            }
          }
        }
      }

      return new Tuple<long, string>(parentCustomerId, parentCustomerName);
    }

  }
}
