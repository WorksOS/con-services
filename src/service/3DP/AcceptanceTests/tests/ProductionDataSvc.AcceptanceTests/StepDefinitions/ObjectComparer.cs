using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  public static class ObjectComparer
  {
    /// <summary>
    /// Iterates all elements in the array, rounding any Doubles to the defined precision.
    /// </summary>
    public static TArray[] RoundAllArrayElementsProperties<TArray>(TArray[] objArray, int roundingPrecision)
    {
      foreach (var a in objArray)
      {
        RoundAllDoubleProperties(a, roundingPrecision);
      }

      return objArray;
    }

    /// <summary>
    /// Rounds all properties on the JObject, identifying and rounding Doubles to the defined precision.
    /// </summary>
    public static void RoundAllDoubleProperties(JObject obj, int roundingPrecision)
    {
      foreach (var jPropertyChild in obj.Properties())
      {
        if (jPropertyChild.Value.Type == JTokenType.Float)
        {
          obj[jPropertyChild.Name] = Math.Round((float)jPropertyChild.Value, roundingPrecision);
          continue;
        }

        WalkNode(jPropertyChild.Value, roundingPrecision);
      }
    }

    private static void WalkNode(JToken node, int roundingPrecision)
    {
      if (node.Type == JTokenType.Object)
      {
        foreach (var jPropertyChild in node.Children<JProperty>())
        {
          WalkNode(jPropertyChild.Value, roundingPrecision);

          if (jPropertyChild.Value.Type != JTokenType.Float)
          {
            continue;
          }

          node[jPropertyChild.Name] = Math.Round((float)jPropertyChild.Value, roundingPrecision);
        }
      }
      else if (node.Type == JTokenType.Array)
      {
        for (var i = 0; i < node.Children().Count(); i++)
        {
          var jToken = node.Children().ElementAt(i);

          if (jToken.Type == JTokenType.Float)
          {
            jToken.Replace(Math.Round(jToken.Value<float>(), roundingPrecision));

            continue;
          }

          WalkNode(jToken, roundingPrecision);
        }
      }
    }

    /// <summary>
    /// Rounds all Double properties to the defined precision.
    /// </summary>
    public static void RoundAllDoubleProperties<T>(T obj, int roundingPrecision)
    {
      foreach (var propertyInfo in obj.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
      {
        if (propertyInfo.PropertyType != typeof(double) && propertyInfo.PropertyType != typeof(double?))
        { 
          continue;
        }

        if (propertyInfo.PropertyType == typeof(double))
        {
          var d = (double)propertyInfo.GetValue(obj);
          var roundedValue = Math.Round(d, roundingPrecision);

          propertyInfo.SetValue(obj, Convert.ChangeType(roundedValue, propertyInfo.PropertyType));
        }

        else if (propertyInfo.PropertyType == typeof(double?))
        {
          var d = (double?)propertyInfo.GetValue(obj);

          var roundedValue = d.HasValue
            ? Math.Round(d.Value, roundingPrecision) as double?
            : null;

          propertyInfo.SetValue(obj, roundedValue);
        }
      }
    }

    /// <summary>
    /// Compares two JObject objects using the JToken::DeepEquals method.
    /// </summary>
    public static void AssertAreEqual(JObject actualResultObj, JObject expectedResultObj, string resultName = "")
    {
      if (!JToken.DeepEquals(expectedResultObj, actualResultObj))
      {
        // Redo the comparison using Assert.Equal because when it fails we'll dump the JSON, rather than simply True or False.
        AssertAreEqual(actualResultObj, expectedResultObj, true, resultName);
      }
       
      Assert.True(true);
    }

    public static void AssertAreEqual(object actualResultObj, object expectedResultObj, bool ignoreCase = false, string resultName = "")
    {
      var actualResultJson = JsonConvert.SerializeObject(actualResultObj, Formatting.None);
      var expectedResultJson = JsonConvert.SerializeObject(expectedResultObj, Formatting.None);

      AssertAreEqual(actualResultJson, expectedResultJson, ignoreCase, resultName);
    }

    public static void AssertAreEqual(string actualResultJson, string expectedResultJson, bool ignoreCase = false, string resultName = "")
    {
      Assert.Equal(expectedResultJson, actualResultJson, ignoreCase: ignoreCase, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
    }

    public static bool CompareDouble(double expectedDouble, double actualDouble, string field, int rowCount, int precision = 6)
    {
      if (Math.Abs(expectedDouble - actualDouble) < precision)
      {
        return true;
      }

      if (Math.Abs(Math.Round(expectedDouble) - Math.Round(actualDouble)) > precision)
      {
        Console.WriteLine($"RowCount:{rowCount} {field} actual: {actualDouble} expected: {expectedDouble}");
        Assert.True(false, $"Expected: {expectedDouble} Actual: {actualDouble} at row index {rowCount} for field {field}");
      }

      return true;
    }
  }
}
