namespace VSS.TRex.Common.Interfaces
{
  /// <summary>
  /// Provides an annotation/interface to indicate that even though the implementer may
  /// implement (even if it does not inherit) the IBinaryReaderWriter interface, it intentionally
  /// declines to support its full semantics, most particularly enforcement of versioning via the
  /// VersionSerializationHelper and IBinaryReaderWriter_Mimic_Tests tests.
  /// </summary>
  public interface INonBinaryReaderWriterMimicable
  {
  }
}
