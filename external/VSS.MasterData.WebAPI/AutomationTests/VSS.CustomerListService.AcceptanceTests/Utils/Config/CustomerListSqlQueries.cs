using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.CustomerListService.AcceptanceTests.Utils.Config
{
    public abstract class CustomerListSqlQueries
    {
        //Customer Service Queries
        public static string CustomerDetailsByCustomerUID = "SELECT CustomerUID, CustomerName, fk_CustomerTypeID FROM Customer where CustomerUID ='";

        public static string CustomerUpdateUTCByCustomerUID = "SELECT LastCustomerUTC FROM Customer where CustomerUID ='";

        public static string CustomerTypeIdByCustomerUID = "SELECT fk_CustomerTypeID FROM Customer where CustomerUID ='";

        public static string CustomerDetailsUpdateByCustomerUID = "SELECT CustomerUID, CustomerName FROM Customer where CustomerUID ='";

        public static string CustomerNameUpdateByCustomerUID = "SELECT CustomerName FROM Customer where CustomerUID ='";


        //Customer User Service Queries
        public static string CustomerUserDetailsByCustomerUID = "SELECT fk_CustomerUID FROM UserCustomer where fk_UserUID='";

        public static string CustomerUserDetailsByUserUID = "SELECT fk_UserUID FROM UserCustomer where fk_CustomerUID='";

        public static string CustomerUserDetails = "SELECT * FROM UserCustomer where fk_CustomerUID='";
    }
}
