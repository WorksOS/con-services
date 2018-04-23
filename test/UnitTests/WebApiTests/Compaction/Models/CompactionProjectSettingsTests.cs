using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.Models
{
  [TestClass]
  public class CompactionProjectSettingsTests
  {

    [TestMethod]
    public void CanCreateProjectSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      //empty project settings
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(settings, out results));

      //full project settings
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int>{1,3,5,8,11,16,20,25});
      Assert.IsTrue(validator.TryValidate(settings, out results));
    }


    [TestMethod]
    public void ValidateSuccessTest()
    {
      //Full custom settings within ranges
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      settings.Validate();

      //target/default flags all true, don't need settings values
      settings = CompactionProjectSettings.CreateProjectSettings(
        true, null, null, true, null, null, true, null, true, null, true, null, null, true, null, null, true, null, null, true, null, true, null, null, true, null);
      settings.Validate();

      //target/default flags all true, can have valid settings
      settings = CompactionProjectSettings.CreateProjectSettings(
        true, 5, 7, true, 75, 155, true, 77, true, 88, true, 75, 105, true, 85, 115, true, 10, 30, true, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, true, 5, 7.5, true, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      settings.Validate();

      //target/default flags all null, don't need settings values
      settings = CompactionProjectSettings.CreateProjectSettings(
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      settings.Validate();

      //target/default flags all null, can have valid settings
      settings = CompactionProjectSettings.CreateProjectSettings(
        null, 5, 7, null, 75, 155, null, 77, null, 88, null, 75, 105, null, 85, 115, null, 10, 30, null, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, null, 5, 7.5, null, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      settings.Validate();
    }



    [TestMethod]
    public void ValidatePassCountSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;

      //minimum pass count out of range
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 0, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //maximum pass count out of range
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 100, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //pass count missing min
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, null, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //pass count missing max
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, null, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //pass count min > max
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 7, 5, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

    }

    [TestMethod]
    public void ValidateTemperatureSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;

      //minimum temperature out of range
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 0, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //maximum temperature out of range
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 1200, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //temperature missing min
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, null, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //temperature missing max
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, null, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //temperature min > max
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 155, 75, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidateCmvPercentSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;

      //minimum CMV % out of range
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 0, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //maximum CMV % out of range
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 300, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //CMV % missing min
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, null, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //CMV % missing max
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, null, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //CMV % min > max
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 105, 75, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidateMdpPercentSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;

      //minimum MDP % out of range
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 0, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //maximum MDP % out of range
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 300, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //MDP % missing min
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, null, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //MDP % missing max
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, null, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //MDP % min > max
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 115, 85, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidateSpeedSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;

      //minimum speed out of range
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 0, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //maximum speed out of range
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 120, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //speed missing min
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, null, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //speed missing max
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, null, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //speed min > max
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 30, 10, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidateCmvSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;

      //CMV out of range
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 1111, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //CMV missing value
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, null, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidateMdpSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;

      //MDP out of range
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 1111, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //MDP missing value
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, null, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidateShrinkageSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;

      //shrinkage out of range
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 101, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //Shrinkage missing value
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, null, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidateBulkingSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;

      //bulking out of range
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 101, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //Bulking missing value
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, null, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidateCutFillSettingsTest()
    {
      //Cut-fill missing values
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, null, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Cut-fill too many values
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 4, 3, 2, 1, 0, -1, -2, -3, -4 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Cut-fill too few values
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 2, 1, 0, -1, -2 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Cut-fill out of range value
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 500, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Cut-fill out of order value
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 1, 2, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Cut-fill on grade not 0
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 4, 3, 2, 1, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidatePassCountDetailsSettingsTest()
    {
      //Pass count missing values
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, null);
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Pass count too many values
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25, 30 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Pass count too few values
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Pass count out of range value
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 500, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 100 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Pass count out of order value
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 20, 16, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Pass count first value not 1
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 0, 3, 5, 8, 11, 16, 20, 25 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidateCmvDetailsSettingsTest()
    {
      //Pass count missing values
      CompactionProjectSettings settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25 }, false, null);
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Pass count too many values
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 25, 30 }, false, new List<int> { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Pass count too few values
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20 }, false, new List<int> { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140});
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Pass count out of range value
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 500, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 16, 20, 100 }, false, new List<int> { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 1510 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Pass count out of order value
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 1, 3, 5, 8, 11, 20, 16, 25 }, false, new List<int> { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 150, 140, 130 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());

      //Pass count first value not 1
      settings = CompactionProjectSettings.CreateProjectSettings(
        false, 5, 7, false, 75, 155, false, 77, false, 88, false, 75, 105, false, 85, 115, false, 10, 30, false, new List<double> { 3, 2, 1, 0, -1, -2, -3 }, false, 5, 7.5, false, new List<int> { 0, 3, 5, 8, 11, 16, 20, 25 }, false, new List<int> { 1, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

  }
}
