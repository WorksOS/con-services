using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VSS.CustomerListService.AcceptanceTests.Utils.Features.Classes.CustomerListService
{
    #region Valid

        public class CustomerListSuccessResponse
        {
            public HttpStatusCode status { get; set; }
            public Metadata metadata { get; set; }
            public List<AssociatedCustomer> customer { get; set; }
        }

        public class Metadata
        {
            public string msg { get; set; }
        }

        public class AssociatedCustomer
        {
            public Guid uid { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

    #endregion
        
    #region InValid

    #endregion
}
