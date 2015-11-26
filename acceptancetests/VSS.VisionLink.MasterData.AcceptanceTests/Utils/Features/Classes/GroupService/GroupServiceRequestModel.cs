using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes
{
    public class CreateGroupServiceModel
    {
        public CreateGroupServiceEvent CreateGroupEvent;
    }

    public class CreateGroupServiceEvent
    {
        public string GroupName { get; set; }
       
        public Guid CustomerUID { get; set; }
       
        public Guid UserUID { get; set; }
        
        public List<Guid> AssetUID { get; set; }
     
        public Guid GroupUID { get; set; }
        
        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }

    }


    public class UpdateGroupServiceModel
    {
        public UpdateGroupServiceEvent UpdateGroupEvent;
    }

    public class UpdateGroupServiceEvent 
    {
      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public string GroupName { get; set; }

      public Guid UserUID { get; set; }
      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public List<Guid> AssociatedAssetUID { get; set; }
      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public List<Guid> DissociatedAssetUID { get; set; }

      public Guid GroupUID { get; set; }

      public DateTime ActionUTC { get; set; }
      public DateTime ReceivedUTC { get; set; }
    }

    public class UpdateGroupService_EmptyGroupNameModel
    {
        public UpdateGroupService_EmptyGroupNameEvent UpdateGroupService_EmptyGroupNameEvent;
    }

    public class UpdateGroupService_EmptyGroupNameEvent
    {

        public Guid UserUID { get; set; }

        public List<Guid> AssociatedAssetUID { get; set; }

        public List<Guid> DissociatedAssetUID { get; set; }

        public Guid GroupUID { get; set; }

        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }


    }

    public class DeleteGroupServiceModel
    {
        public DeleteGroupServiceEvent DeleteGroupEvent;
    }

    public class DeleteGroupServiceEvent
    {

        public Guid UserUID { get; set; }
   
        public Guid GroupUID { get; set; }

        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }

    }

    public class InValidCreateGroupServiceEvent
    {
        public string GroupName { get; set; }

        public string CustomerUID { get; set; }

        public string UserUID { get; set; }

        public string AssetUID { get; set; }

        public string GroupUID { get; set; }

        public string ActionUTC { get; set; }

        public string ReceivedUTC { get; set; }

    }

    public class InValidUpdateGroupServiceEvent
    {

        public string GroupName { get; set; }

        public string UserUID { get; set; }

        public string AssociatedAssetUID { get; set; }

        public string DissociatedAssetUID { get; set; }

        public string GroupUID { get; set; }

        public string ActionUTC { get; set; }
        public string ReceivedUTC { get; set; }


    }

    public class InValidDeleteGroupServiceEvent
    {

        public string UserUID { get; set; }

        public string GroupUID { get; set; }

        public string ActionUTC { get; set; }

        public string ReceivedUTC { get; set; }

    }




}
