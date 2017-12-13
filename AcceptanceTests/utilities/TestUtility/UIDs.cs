using System;

namespace TestUtility
{
  public class UIDs
  {
    // The userId from JWT token in RestClientUtil.cs so we can easily test the data using Postman requests.
    public static string JWT_USER_ID = "98cdb619-b06b-4084-b7c5-5dcccc82af3b";

    // For ease of testing during development, the ID mataches that from the Dimenions project in Mock Web API.
    public static readonly Guid MOCK_WEB_API_DIMENSIONS_PROJECT_UID = Guid.Parse("ff91dd40-1569-4765-a2bc-014321f76ace");
  }
}