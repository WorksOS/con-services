using System;
using System.Collections.Generic;

namespace VSS.Nighthawk.MasterDataSync.Models
{
    public class APIUserEvent
    {
        public Guid Customeruid { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string UserSalt { get; set; }
        public Guid? TPaasAppUID { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public DateTime LastLoginUserUTC { get; set; }
        public Guid FeedLoginUserUID { get; set; }
        public List<string> UserFeatures { get; set; }
        public string Operation { get; set; }
    }
}