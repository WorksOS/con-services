using System;

using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public static class BssMessageExtensions
  {
    public static DealerNetworkEnum ToDealerNetworkEnum(this string bssDealerNetworkAsString)
    {
      if (string.IsNullOrEmpty(bssDealerNetworkAsString)) 
        return DealerNetworkEnum.None;

      switch (bssDealerNetworkAsString.ToUpper())
      {
        case "CAT":
          return DealerNetworkEnum.CAT;

        case "SITECH":
          return DealerNetworkEnum.SITECH;

        case "TRIMBLE":
          return DealerNetworkEnum.TRMB;

        case "TATA HITACHI":
          return DealerNetworkEnum.THC;

        case "LEEBOY":
          return DealerNetworkEnum.LEEBOY;

        case "CASE":
          return DealerNetworkEnum.CASE;

        case "NEW HOLLAND":
          return DealerNetworkEnum.NEWHOLLAND;

        case "DOOSAN":
          return DealerNetworkEnum.DOOSAN;

        case "LTCEL":
          return DealerNetworkEnum.LTCEL;

        case "LIUGONG":
          return DealerNetworkEnum.LGI;

        case "TRIMBLE AG":
          return DealerNetworkEnum.TRMBAG;

        case "LTCM":
          return DealerNetworkEnum.LTCM;

        case "ALCV":
          return DealerNetworkEnum.ALCV;

        case "NONE":
          return DealerNetworkEnum.None;
          
        default:
          throw new InvalidOperationException("BSS DealerNetwork string can not be converted to DealerNetworkEnum.");
      }
    }

    public static CustomerRelationshipTypeEnum ToCustomerRelationshipTypeEnum(this string hierarchyTypeAsString)
    {
      switch (hierarchyTypeAsString.ToUpper())
      {
        case "TCS DEALER":
          return CustomerRelationshipTypeEnum.TCSDealer;

        case "TCS CUSTOMER":
          return CustomerRelationshipTypeEnum.TCSCustomer;

        default:

          throw new InvalidOperationException("BSS HierarchyType string cannot be converted to CustomerTypeRelationshipEnum.");
      }
    }
  }
}