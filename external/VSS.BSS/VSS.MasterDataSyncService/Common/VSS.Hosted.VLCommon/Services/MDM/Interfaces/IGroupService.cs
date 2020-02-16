using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon
{
	public interface IGroupService
	{
		bool CreateGroup(object groupDetails);
		bool UpdateGroup(object groupDetails);
		bool DeleteGroup(DeleteGroupEvent groupDetails, string url);
	}
}
