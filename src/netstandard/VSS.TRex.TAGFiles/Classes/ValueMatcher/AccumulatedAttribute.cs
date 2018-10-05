using System;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher
{
  /// <summary>
  /// AccumulatedAttribute records the state of an attribute supplied to the
  /// snail trail processor, in conjunction with the date/time it was recorded
  /// </summary>
  public struct AccumulatedAttribute
  {
    public DateTime dateTime;
    public object value;

    public AccumulatedAttribute(DateTime dateTime, object value)
    {
      this.dateTime = dateTime;
      this.value = value;
    }
  }
}
