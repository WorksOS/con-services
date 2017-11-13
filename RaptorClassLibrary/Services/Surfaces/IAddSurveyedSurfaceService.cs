using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Services.Surfaces
{
    /// <summary>
    /// Interface detailing the API for the service that supports adding new surveyed surfaces
    /// </summary>
    public interface IAddSurveyedSurfaceService
    {
        void Add(long SiteModelID, DesignDescriptor designDescriptor, DateTime AsAtDate);
    }
}
