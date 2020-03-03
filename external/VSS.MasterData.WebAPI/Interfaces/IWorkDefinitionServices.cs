using ClientModel.WorkDefinition;
using DbModel.WorkDefinition;
using System;

namespace Interfaces
{
	public interface IWorkDefinitionServices
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetUID"></param>
		/// <returns></returns>
		bool WorkDefinitionExist(Guid assetUID);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workDefinition"></param>
		/// <returns></returns>
		bool CreateWorkDefinition(WorkDefinitionEvent workDefinition);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workDefinition"></param>
		/// <returns></returns>
		bool UpdateWorkDefinition(WorkDefinitionEvent workDefinition);

		long GetWorkDefinitionTypeID(string workDefinitionType);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetUID"></param>
		/// <returns></returns>
		WorkDefinitionDto GetWorkDefinition(Guid assetUID);
	}
}
