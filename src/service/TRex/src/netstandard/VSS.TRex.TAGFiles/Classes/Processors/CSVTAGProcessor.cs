using System;
using System.IO;
using CsvHelper;
using VSS.TRex.Geometry;
using VSS.TRex.TAGFiles.Classes.ValueMatcher;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.Processors
{
  public class CSVTAGProcessor : TAGProcessorBase
  {

    private string[] _datalist;
    private StreamWriter _writer;
    private CsvWriter _csv;
    private bool _disposed;


    public CSVTAGProcessor(string[] datalist, string tagfilename)
    {
      _datalist = datalist;
      _writer = new StreamWriter($"{tagfilename.Replace(".tag",".csv")}");
      _csv = new CsvWriter(_writer);
      
      ///Write Header
      for (var i = 0; i < _datalist.Length; i++)
      {
        var value = GetPropValue(this, _datalist[i]);
        if (value.GetType() == typeof(XYZ))
        {
          _csv.WriteField(_datalist[i] + "_X");
          _csv.WriteField(_datalist[i] + "_Y");
          _csv.WriteField(_datalist[i] + "_Z");
        }
        else
        {
          _csv.WriteField(_datalist[i]);
        }
          
      }
      _csv.NextRecord();

    }

    public override bool DoEpochPreProcessAction()
    {
      return true;
    }

    public override void DoPostProcessFileAction(bool successState)
    {
      Console.WriteLine("PostProcessFile");
    }

    public override void DoProcessEpochContext(Fence InterpolationFence, MachineSide machineSide)
    {
      foreach (var s in _datalist)
      {
        var value = GetPropValue(this, s);
        if (value.GetType() == typeof(XYZ)) {
          _csv.WriteField(((XYZ)value).X);
          _csv.WriteField(((XYZ)value).Y);
          _csv.WriteField(((XYZ)value).Z);
        }
        //There should be a tidier way of doing this but this will do for now.
        else if (typeof(AccumulatedAttributes<>).Name.Equals(value.GetType().Name))
        {
          var type = value.GetType().GetGenericArguments()?[0];
          var instance = typeof(AccumulatedAttributes<>).MakeGenericType(type);
          var method = instance.GetMethod("GetLatest");
          var latest = method?.Invoke(value, null);
          _csv.WriteField(latest);
        }
        else
        {
          _csv.WriteField(GetPropValue(this, s));
        }
      }
      _csv.NextRecord();
      _writer.Flush();
    }

    public static object GetPropValue(object obj, string name)
    {
      if (obj == null) { return null; }
      var type = obj.GetType();
      foreach (var part in name.Split('.'))
      {
        var property = type.GetProperty(part);

        if (property == null) {
          var field = type.GetField(part);
          obj = field.GetValue(obj);
        }
        else
        {
          obj = property.GetValue(obj, null);
        }
        
      }
      return obj;
    }

    #region Cleanup

    protected override void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        if (disposing)
        {
          _writer?.Dispose();
          _csv?.Dispose();
          base.Dispose(disposing);
        }
        _disposed = true;
      }
    }

    public new void Dispose()
    {
      Dispose(true);
    }
    
    #endregion
  }
}
