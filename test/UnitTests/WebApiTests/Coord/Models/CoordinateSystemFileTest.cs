
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.WebApiModels.Coord.Models;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiTests.Coord.Controllers
{
    [TestClass]
    public class CoordinateSystemFileTest
    {
        [TestMethod]
        public void CanCreateCoordinateSystemFileTest()
        {
            var validator = new DataAnnotationsValidator();

            ICollection<ValidationResult> results;

            string fileName = "test.dc";

            byte[] fileContent = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

            // Test the CreateCoordinateSystemFile() method with valid parameters... 
            CoordinateSystemFile coordSystemFile = CoordinateSystemFile.CreateCoordinateSystemFile(1, fileContent, "test.dc");
            Assert.IsTrue(validator.TryValidate(coordSystemFile, out results));

            // Test the CreateCoordinateSystemFile() method with an invalid projectID... 
            coordSystemFile = CoordinateSystemFile.CreateCoordinateSystemFile(-1, fileContent, fileName);
            Assert.IsFalse(validator.TryValidate(coordSystemFile, out results));

            // Test the CreateCoordinateSystemFile() method with a file name length exceeds 256 characters... 
            string prefix = "overlimit";
            //int maxCount = (int)(CoordinateSystemFile.MAX_FILE_NAME_LENGTH / prefix.Length);

            for (int i = 1; prefix.Length <= CoordinateSystemFile.MAX_FILE_NAME_LENGTH; i++)
                prefix += prefix;

            coordSystemFile = CoordinateSystemFile.CreateCoordinateSystemFile(1, fileContent, prefix + fileName);
            Assert.IsFalse(validator.TryValidate(coordSystemFile, out results));

            // Test the CreateCoordinateSystemFile() method with no file name provided... 
            coordSystemFile = CoordinateSystemFile.CreateCoordinateSystemFile(1, fileContent, string.Empty);
            Assert.IsFalse(validator.TryValidate(coordSystemFile, out results));

            // Test the CreateCoordinateSystemFile() method with no content provided... 
            coordSystemFile = CoordinateSystemFile.CreateCoordinateSystemFile(1, null, fileName);
            Assert.IsFalse(validator.TryValidate(coordSystemFile, out results));
        }
    }
}
