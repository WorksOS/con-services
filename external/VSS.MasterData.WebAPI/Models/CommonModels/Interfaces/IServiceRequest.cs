using System;

namespace ClientModel.Interfaces
{
	public interface IServiceRequest
	{
		Guid? CustomerUid { get; set; }
		Guid? UserUid { get; set; }
	}
}

