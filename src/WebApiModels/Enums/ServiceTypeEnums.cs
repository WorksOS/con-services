using System.Collections.Generic;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums
{

  // Service type enums are different between CG and NG.
  //   Luckily the strings appear to be the same at this stage.
  //   Raptor requires CG numbers, so we need to map from our Db NG for it.
  public class ServiceTypeMapping
  {
    public string name = "";
    public int CGEnum = 0;
    public int NGEnum = 0;
  }

  public class ServiceTypeMappings
  {
    public List<ServiceTypeMapping> serviceTypes;
    public ServiceTypeMappings()
    {
      serviceTypes = new List<ServiceTypeMapping>();
      serviceTypes.Add(new ServiceTypeMapping() { name = "Unknown", CGEnum = 0, NGEnum = 0 });

      // asset-based sub
      serviceTypes.Add(new ServiceTypeMapping() { name = "3D Project Monitoring", CGEnum = 16, NGEnum = 13 });

      // customer-based sub
      serviceTypes.Add(new ServiceTypeMapping() { name = "Manual 3D Project Monitoring", CGEnum = 18, NGEnum = 15 });

      // project-based subs
      serviceTypes.Add(new ServiceTypeMapping() { name = "Landfill", CGEnum = 23, NGEnum = 19 });
      serviceTypes.Add(new ServiceTypeMapping() { name = "Project Monitoring", CGEnum = 24, NGEnum = 20 });
    }
  }  

}
