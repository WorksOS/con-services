using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.AccountHierarchyWebAPI
{
  public class AccountHierarchyWebAPIDBQueries
  {
    public static string GetHierarchyByUserUID_Dealer = "SELECT hex(CUstomerUID) CustomerUID,CustomerName,fk_CustomerTypeID CustomerTypeID,NetworkDealerCode from Customer where CustomeruID in (SELECT (fk_CustomerUID) FROM `VSS-Customer-ALPHA`.UserCustomer where fk_UserUID=unhex('9cac16b8f6db46c6949e8fc122867695'))";
    public static string GetHierarchyByUserUID_Customer = "SELECT hex(C.CustomerUID) CustomerUID,C.CustomerName CustomerName,C.fk_CustomerTypeID CustomerTypeID,CA.NetworkCUstomerCode NetworkCustomerCode from Customer C inner Join CustomerAccount CA on C.CustomerUID=CA.fk_ChildCustomerUID inner join UserCustomer UC on C.CustomerUID=UC.fk_CustomerUID where UC.fk_UserUID=unhex('9cac16b8f6db46c6949e8fc122867695')";

  }
}
