using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.MasterData.WebAPI.ClientModel
{
    public class AssetCustomer
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid CustomerUID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CustomerType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? ParentCustomerUID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ParentName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ParentCustomerType { get; set; }

    }
}