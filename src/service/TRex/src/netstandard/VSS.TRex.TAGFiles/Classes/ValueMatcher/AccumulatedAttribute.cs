using System;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher
{
  /// <summary>
  /// AccumulatedAttribute records the state of an attribute supplied to the
  /// snail trail processor, in conjunction with the date/time it was recorded
  /// </summary>
  public struct AccumulatedAttribute<T>
  {
    public DateTime dateTime;
    public T value;

    public AccumulatedAttribute(DateTime dateTime, T value)
    {
      this.dateTime = dateTime;
      this.value = value;
    }

    public void Set(DateTime date, T val)
    {
      dateTime = date;
      value = val;
    }
  }
}
