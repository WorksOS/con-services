namespace CoreXModels
{
  public struct Datum
  {
    public int SystemId { get; }
    public int Type { get; }
    public string Name { get; }

    public Datum(int datumSystemId, int datumType, string datumName)
    {
      SystemId = datumSystemId;
      Type = datumType;
      Name = datumName;
    }
  }
}
