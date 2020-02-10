using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.AccountHierarchy
{
  public class DBQueries
  {
   public static string AccountHierarchyValidation = "SELECT count(1) FROM CustomerRelationshipNode where fk_RootCustomerUID=unhex('{0}') and fk_ParentCustomerUID=unhex('{1}') and fk_CustomerUID=unhex('{2}') and LeftNodePosition={3} and RightNodePosition={4}";

    public static string AccountHierarchyCountByRootNodeUID = "SELECT count(1) from CustomerRelationshipNode where fk_RootCustomerUID=unhex('{0}')";

}
}
