using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using VSS.VisionLink.Raptor.TAGFiles.Classes.Sinks;

namespace VSS.VisionLink.Raptor.TAGFiles.Tests
{
    [TestClass]
    public class TAGValueNameTests
    {
        [TestMethod]
        public void Test_TAGValueNames_GetTAGValueNames()
        {
            // Determine that the set of defined TAG value names are fully used by the defined TAGValueMatchers

            // Get a list of all the TAG value string names from the TAGValueNames class that defines all the string constants
            List<string> valueNames = TagValueNamesArray.Names();

            // Instantiate a TAGValueSink and ask it for the list of TAGs supproted
            string[] instantiatedTAGs = new TAGValueSink(null).InstantiatedTAGs;

//            Assert.Fail("Instantiated TAGs! : {0}, compared to {1}", String.Join(", ", instantiatedTAGs), String.Join(", ", valueNames));
            Assert.IsTrue(instantiatedTAGs != null || instantiatedTAGs.Length > 0, "No instantiated TAG!");

            List<string> missing = new List<string>();

            // Compare the two lists to see which ones are missing
            foreach(string s in valueNames)
            {
                bool found = false;

                foreach (string ss in instantiatedTAGs)
                {
                    if (ss.Equals(s))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    missing.Add(s);
                }
            }

            Assert.IsTrue(missing.Count == 0, "Missing TAGs: {0}", String.Join(", ", missing));
            // Spot check a couple of names...
//            Assert.IsTrue(valueNames.Contains("TIME") && valueNames.Contains("WEEK"), "TAG value names list does not contain expected names");
//            Assert.IsTrue(String.Join(", ", valueNames) != String.Empty);
            //Assert.Fail(String.Join(", ", valueNames));
        }
    }
}
