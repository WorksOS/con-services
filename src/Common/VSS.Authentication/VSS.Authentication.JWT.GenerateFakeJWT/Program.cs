using System;

namespace VSS.Authentication.JWT.GenerateFakeJWT
{
  public class Program
  {
    public static void Main(string[] args)
    {
      if (args.Length < 1)
      {
        Console.WriteLine("Please pass in a user id as a command line parameter (e.g. dotnet run cb0ebe8a-b960-4f5d-8552-0439f5f0dfe6)");
        Environment.Exit(1);
      }

      var jwt = TPaaSJWT.GenerateFakeApplicationUserJWT(Guid.Parse(args[0]));
      Console.WriteLine($"X-JWT-Assertion: {jwt.EncodedJWT}");
    }
  }
}
