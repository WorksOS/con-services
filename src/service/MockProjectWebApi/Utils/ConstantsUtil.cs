using System;

namespace MockProjectWebApi.Utils
{
  public static class ConstantsUtil
  {
    public const int DIMENSIONS_PROJECT_ID = 1001158;
    public const string DIMENSIONS_PROJECT_UID = "ff91dd40-1569-4765-a2bc-014321f76ace";

    public const int CUSTOM_SETTINGS_DIMENSIONS_PROJECT_ID = 1001160;
    public const string CUSTOM_SETTINGS_DIMENSIONS_PROJECT_UID = "3335311a-f0e2-4dbe-8acd-f21135bafee4";

    //Empty project has no production data or filess
    public const int DIMENSIONS_EMPTY_PROJECT_ID = 1001157;
    public const string DIMENSIONS_EMPTY_PROJECT_UID = "290df997-7331-405f-ac9c-bebd193965e0";


    //These are used for imported files and surveyed surfaces tests
    public const int GOLDEN_DATA_DIMENSIONS_PROJECT_ID_1 = 1007777;
    public const string GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1 = "7925f179-013d-4aaf-aff4-7b9833bb06d6";
    public const int GOLDEN_DATA_DIMENSIONS_PROJECT_ID_2 = 1007778;
    public const string GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2 = "86a42bbf-9d0e-4079-850f-835496d715c5";

    public const int LANDFILL_PROJECT_ID = 385;
    public const string LANDFILL_PROJECT_UID = "e1f85c4d-04eb-463c-9a5b-9644c96e75ca";
  }

  //public class Project
  //{
  //  public static Guid DIMENSIONS_PROJECT_UID => Guid.Parse("ff91dd40-1569-4765-a2bc-014321f76ace");
  //  public static Guid GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1 => Guid.Parse("7925f179-013d-4aaf-aff4-7b9833bb06d6");
  //  public static Guid GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2 => Guid.Parse("86a42bbf-9d0e-4079-850f-835496d715c5");
  //}
}
