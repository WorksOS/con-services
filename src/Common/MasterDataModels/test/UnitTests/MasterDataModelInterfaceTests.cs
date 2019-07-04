using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;
using Xunit;

namespace VSS.MasterData.Models.UnitTests
{
  public class MasterDataModelInterfaceTests
  {
    [Fact]
    public void TestAllModelsImplementInterface()
    {
      // Any model that implements IMasterDataModel MUST return a list, either empty or populated
      // It can not be null, as the model can be contained within larger models which aggregate the Identifiers - it's easier to return an empty list, than check for null every time you query related models
      var type = typeof(IMasterDataModel);

      // BaseDataResult is in master data models, to make sure we get the right assembly
      var assembly = Assembly.GetAssembly(typeof(BaseDataResult));

      // We need to be checking the master data models assembly
      Assert.Equal("VSS.MasterData.Models", assembly.GetName().Name);

      var types = assembly
        .GetTypes()
        .Where(p => type.IsAssignableFrom(p))
        .ToList();

      var failed = new List<string>();

      foreach (var type1 in types)
      {
        Console.WriteLine($"Testing type {type1.Name} for {nameof(IMasterDataModel.GetIdentifiers)}");
        var dataModel = Activator.CreateInstance(type1) as IMasterDataModel;
        Assert.NotNull(dataModel);
        if(dataModel.GetIdentifiers() == null)
          failed.Add(type1.Name);
      }

      Assert.True(failed.Count == 0, $"Failed to validate '{string.Join(", ", failed)}' models - they have null {nameof(IMasterDataModel.GetIdentifiers)}");
    }
  }
}
