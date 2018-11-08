using System;

namespace VSS.MasterData.Repositories.DBModels
{
    public class Subscription
    {
        public string SubscriptionUID { get; set; }
        public string CustomerUID { get; set; }
        public int ServiceTypeID { get; set; }

        // start, end and Effective are actually only date with no time component. However C# has no date-only.
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; } = DateTime.MaxValue.Date;

        public DateTime LastActionedUTC { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Subscription;

            if (other == null)
                return false;

            return SubscriptionUID == other.SubscriptionUID &&
                   CustomerUID == other.CustomerUID &&
                   ServiceTypeID == other.ServiceTypeID &&
                   StartDate == other.StartDate &&
                   EndDate == other.EndDate &&
                   LastActionedUTC == other.LastActionedUTC;
        }

        public override int GetHashCode()
        {
            return SubscriptionUID.GetHashCode() +
                   CustomerUID.GetHashCode() +
                   ServiceTypeID.GetHashCode() +
                   StartDate.GetHashCode() +
                   EndDate.GetHashCode() +
                   LastActionedUTC.GetHashCode();
        }
    }

    public class ServiceType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int ServiceTypeFamilyID { get; set; }
        public string ServiceTypeFamilyName { get; set; }
    }
}