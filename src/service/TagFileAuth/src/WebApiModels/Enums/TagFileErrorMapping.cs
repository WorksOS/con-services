using System.Collections.Generic;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums
{

  // Tag file error enums are different between raptor and notifications
  public class TagFileErrorMapping
  {
    public string name = "";
    public int RaptorEnum = 0;
    public int NotificationEnum = 0;
  }

  public class TagFileErrorMappings
  {
    public List<TagFileErrorMapping> tagFileErrorTypes;
    public TagFileErrorMappings()
    {
      tagFileErrorTypes = new List<TagFileErrorMapping>();
      tagFileErrorTypes.Add(new TagFileErrorMapping() { name = "Unknown", RaptorEnum = 0, NotificationEnum = 0 });
      tagFileErrorTypes.Add(new TagFileErrorMapping() { name = "ProjectID_NoMatchingDateTime", RaptorEnum = 1, NotificationEnum = 1 });
      tagFileErrorTypes.Add(new TagFileErrorMapping() { name = "ProjectID_NoMatchingArea", RaptorEnum = 2, NotificationEnum = 2 });
      tagFileErrorTypes.Add(new TagFileErrorMapping() { name = "ProjectID_MultipleProjects", RaptorEnum = 3, NotificationEnum = 3 });
      tagFileErrorTypes.Add(new TagFileErrorMapping() { name = "ProjectID_InvalidLLHNEPosition", RaptorEnum = 4, NotificationEnum = 4 });
      tagFileErrorTypes.Add(new TagFileErrorMapping() { name = "NoValidCells_InValidPosition", RaptorEnum = 5, NotificationEnum = 5 });
      tagFileErrorTypes.Add(new TagFileErrorMapping() { name = "InvalidPosition", RaptorEnum = 6, NotificationEnum = 6 });
      tagFileErrorTypes.Add(new TagFileErrorMapping() { name = "UnknownCell", RaptorEnum = -1, NotificationEnum = 7 });
      tagFileErrorTypes.Add(new TagFileErrorMapping() { name = "UnknownProject", RaptorEnum = -2, NotificationEnum = 8 });
    }
  }  

}
