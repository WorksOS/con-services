using System;
using System.Runtime.Serialization;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Implementations.Helpers
{
  #region Unknown Reference Exceptions

  public class UnknownAssetReferenceException : Exception
  {
    public UnknownAssetReferenceException(string message) : base(message) { }
    protected UnknownAssetReferenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  public class UnknownDeviceReferenceException : Exception
  {
    public UnknownDeviceReferenceException(string message) : base(message) { }
    protected UnknownDeviceReferenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  public class UnknownCustomerReferenceException : Exception
  {
    public UnknownCustomerReferenceException(string message) : base(message) { }
    protected UnknownCustomerReferenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  public class UnknownServiceReferenceException : Exception
  {
    public UnknownServiceReferenceException(string message) : base(message) { }
    protected UnknownServiceReferenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  #endregion

  #region Found Duplicate Reference Exceptions

  public abstract class DuplicateReferenceFoundException : Exception
  {
    protected DuplicateReferenceFoundException(string referenceType) :
      base(String.Format("Duplicate {0} reference found.", referenceType)) {}
    protected DuplicateReferenceFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  public class DuplicateAssetReferenceFoundException : DuplicateReferenceFoundException
  {
    public DuplicateAssetReferenceFoundException() : base("Asset") { }
    protected DuplicateAssetReferenceFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  public class DuplicateDeviceReferenceFoundException : DuplicateReferenceFoundException
  {
    public DuplicateDeviceReferenceFoundException() : base("Device") { }
    protected DuplicateDeviceReferenceFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  public class DuplicateCustomerReferenceFoundException : DuplicateReferenceFoundException
  {
    public DuplicateCustomerReferenceFoundException() : base("Customer") { }
    protected DuplicateCustomerReferenceFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  public class DuplicateServiceReferenceFoundException : DuplicateReferenceFoundException
  {
    public DuplicateServiceReferenceFoundException() : base("Subscription") { }
    protected DuplicateServiceReferenceFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  #endregion

  #region Creating Duplicate Reference Exceptions

  public abstract class CreatingDuplicateException : Exception
  {
    protected CreatingDuplicateException(string referenceType) :
      base(String.Format("Attempt to create duplicate {0} reference.", referenceType)) { }
    protected CreatingDuplicateException(SerializationInfo info, StreamingContext context) : base(info, context) {}
  }

  public class CreatingDuplicateAssetReferenceException : CreatingDuplicateException
  {
    public CreatingDuplicateAssetReferenceException() : base("Asset") { }
    protected CreatingDuplicateAssetReferenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  public class CreatingDuplicateDeviceReferenceException : CreatingDuplicateException
  {
    public CreatingDuplicateDeviceReferenceException() : base("Device") { }
    protected CreatingDuplicateDeviceReferenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  public class CreatingDuplicateCustomerReferenceException : CreatingDuplicateException
  {
    public CreatingDuplicateCustomerReferenceException() : base("Customer") { }
    protected CreatingDuplicateCustomerReferenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  public class CreatingDuplicateServiceReferenceException : CreatingDuplicateException
  {
    public CreatingDuplicateServiceReferenceException() : base("Subscription") { }
    protected CreatingDuplicateServiceReferenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  #endregion
}
