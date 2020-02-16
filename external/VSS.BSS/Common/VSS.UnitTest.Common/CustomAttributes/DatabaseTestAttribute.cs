using System;
using System.Transactions;

namespace VSS.UnitTest.Common
{
  [AttributeUsage(AttributeTargets.Method,AllowMultiple = false)]
  public class DatabaseTestAttribute : Attribute
  {
    private TransactionScopeOption _transactionScopeOption;
    private TransactionOptions _transactionOptions;

    public DatabaseTestAttribute() : 
    this(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted, Timeout = TimeSpan.FromSeconds(180) })
    {}

    public DatabaseTestAttribute(TransactionScopeOption transactionScopeOption, TransactionOptions transactionOptions)
    {
        _transactionScopeOption = transactionScopeOption;
        _transactionOptions = transactionOptions;
    }

    public TransactionScopeOption TransactionScopeOption
    {
      get { return _transactionScopeOption; }
    }

    public TransactionOptions TransactionOptions
    {
      get { return _transactionOptions; }
    }

  }
}
