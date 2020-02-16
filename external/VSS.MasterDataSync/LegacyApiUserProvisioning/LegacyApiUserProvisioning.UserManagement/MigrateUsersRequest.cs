

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyApiUserProvisioning.UserManagement.Interfaces;

namespace LegacyApiUserProvisioning.UserManagement
{
	public class MigrateUsersRequestDto : IMigrateUsersRequest
	{
		public string CustomerUid { get; set; }

		public string[] UserIds { get; set; }
	}
}
