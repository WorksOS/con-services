using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSP.MasterData.Project.AcceptanceTests.Utils;
using VSP.MasterData.Project.AcceptanceTests.Models.Customer;
using VSP.MasterData.Project.AcceptanceTests.Models.UserCustomer;

namespace VSP.MasterData.Project.AcceptanceTests.Scenarios.ScenarioSupports
{
    public class MasterDataSupport
    {
        public CreateCustomerEvent CreateCustomerEvt;
        public AssociateCustomerUserEvent AssociateCustomerUserEvt;

        public string CreateCustomer(Guid customerUid)
        {
            CreateCustomerEvt = new CreateCustomerEvent
            {
                CustomerUID = customerUid,
                CustomerName = "AT_CUS-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                CustomerType = CustomerType.Corporate,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { CreateCustomerEvent = CreateCustomerEvt },
               new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        public string AssociateCustomerUser(Guid customerUid, Guid userUid)
        {
            AssociateCustomerUserEvt = new AssociateCustomerUserEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = customerUid,
                UserUID = userUid
            };

            return JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = AssociateCustomerUserEvt },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
    }
}
