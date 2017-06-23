using System;

namespace TestRun
{
  public class Program
    {
        public static void Main(string[] args)
        {
            var testController = new TestControl();

            if (args.Length < 2)
            {
                Console.WriteLine("Failed to run tests. Have you passed all the arguments");
                Environment.Exit(1);
                return;
            }

            var isAllPassed = testController.RunAllTests(args);

            if (isAllPassed)
            {
                Console.WriteLine("Tests have all PASSED ");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Tests have FAILED ");
                Environment.Exit(1);
            }
        }
    }
}