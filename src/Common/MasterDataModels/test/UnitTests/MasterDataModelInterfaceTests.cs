using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.UnitTests
{
  [TestClass]
  public class MasterDataModelInterfaceTests
  {
    [TestMethod]
    public void TestAllModelsImplementInterface()
    {
      // Any model that implements IMasterDataModel MUST return a list, either empty or populated
      // It can not be null, as the model can be contained within larger models which aggregate the Identifiers - it's easier to return an empty list, than check for null every time you query related models
      var type = typeof(IMasterDataModel);

      // Filter Data is in master data models, to make sure we get the right assembly
      var assembly = Assembly.GetAssembly(typeof(FilterData));

      // We need to be checking the master data models assembly
      Assert.AreEqual("VSS.MasterData.Models", assembly.GetName().Name);

      var types = assembly
        .GetTypes()
        .Where(p => type.IsAssignableFrom(p))
        .ToList();

      var failed = new List<string>();

      foreach (var type1 in types)
      {
        Console.WriteLine($"Testing type {type1.Name} for {nameof(IMasterDataModel.GetIdentifiers)}");
        var dataModel = Activator.CreateInstance(type1) as IMasterDataModel;
        Assert.IsNotNull(dataModel, $"Could not create type: {type1.Name}. This should have a default constructor as it's a datamodel that's passed between services");
        if(dataModel.GetIdentifiers() == null)
          failed.Add(type1.Name);
      }

      Assert.IsTrue(failed.Count == 0, $"Failed to validate '{string.Join(", ", failed)}' models - they have null {nameof(IMasterDataModel.GetIdentifiers)}");

    }
  }
}