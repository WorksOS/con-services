using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Config
{
  public abstract class CustomerServiceMySqlQueries
  {


    //Customer Service Queries
    public static string CustomerDetailsByCustomerUID = "SELECT hex(CustomerUID), CustomerName, fk_CustomerTypeID, PrimaryContactEmail, FirstName, LastName FROM Customer where CustomerUID = unhex('";

    public static string CustomerUpdateUTCByCustomerUID = "SELECT LastCustomerUTC FROM Customer where CustomerUID = unhex('";

    public static string CustomerTypeIdByCustomerUID = "SELECT fk_CustomerTypeID FROM Customer where CustomerUID ='";

    public static string CustomerDetailsUpdateByCustomerUID = "SELECT CustomerUID, CustomerName FROM Customer where CustomerUID ='";

    public static string CustomerNameUpdateByCustomerUID = "SELECT CustomerName FROM Customer where CustomerUID ='";

    public static string CustomerDetails = "SELECT hex(CustomerUID) FROM Customer limit 1";


    //Customer User Service Queries
    public static string CustomerUserDetailsByCustomerUID = "SELECT fk_CustomerUID FROM UserCustomer where fk_UserUID='";

    public static string CustomerUserDetailsByUserUID = "SELECT fk_UserUID FROM UserCustomer where fk_CustomerUID='";

    public static string CustomerUserDetails = "SELECT * FROM UserCustomer where fk_CustomerUID='";

    public static string CustomerUserDetailsLimit = "SELECT fk_CustomerUID,fk_UserUID FROM UserCustomer limit 1";


    //Customer Asset Service Queries
    public static string CustomerAssetDetailsByCustomerUID = "SELECT fk_CustomerUID,fk_AssetUID,fk_AssetRelationTypeID FROM AssetCustomer where fk_CustomerUID='";

    public static string CustomerAssetDetails = "SELECT fk_CustomerUID,fk_AssetUID FROM AssetCustomer limit 1";


    //Customer Relationship Service Queries

    public static string CustomerRelationshipByParentCustomerUID = "SELECT fk_ParentCustomerUID,fk_CustomerUID,fk_RootCustomerUID,LeftNodePosition,RightNodePosition FROM CustomerRelationshipNode where fk_ParentCustomerUID='";

    public static string CustomerRelationshipByChildCustomerUID = "SELECT fk_ParentCustomerUID,fk_CustomerUID,fk_RootCustomerUID,LeftNodePosition,RightNodePosition FROM CustomerRelationshipNode where fk_CustomerUID='";

  }

}

