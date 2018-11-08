using System;

namespace VSS.MasterData.Repositories.DBModels
{
    public class CustomerProject
    {
        public string ProjectUID { get; set; }
        public string CustomerUID { get; set; }

        // this belongs in Customer table, however for expediancy it arrives in the CustomerProject kafka Event.
        public long LegacyCustomerID { get; set; }

        public DateTime LastActionedUTC { get; set; }
    }
}