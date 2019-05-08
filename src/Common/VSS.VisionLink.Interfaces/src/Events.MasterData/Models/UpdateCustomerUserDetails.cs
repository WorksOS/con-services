using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class UpdateCustomerUserDetails
    {
        public Guid CustomerUID { get; set; }
        public Guid UserUID { get; set; }
        /// <summary>
        /// This field holds the Job Tile of an User within the Customer. For Ex:- Equipment Manager, Sales Manager etc.,
        /// </summary>
        public string JobTitle { get; set; }
        /// <summary>
        /// This field holds the JobType of an User within the Customer. For Ex:- Employee, Non Employee etc., 
        /// </summary>
        public string JobType { get; set; }
        /// <summary>
        /// This field holds the UserUID of the user who created this Customer-User relation. Empty Guid will be used to refer System created Customer-Users.
        /// </summary>
        public Guid? CreatedByUserUID { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
