using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM.Interfaces
{
    public interface IWorkDefinitionService
    {
        bool CreateWorkDefinition(object workdefinitionDetails);
        bool UpdateWorkDefinition(object workdefinitionDetails);
    }
}
