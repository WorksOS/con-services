using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace VSS.Hosted.VLCommon
{
  public class ObjectContextTransactionScope : IObjectContextTransactionScope
  {
    /*************************************************************************************
     * NOTE: This class attempts to approach an abstraction of a unified transaction scope 
     * without encountering the MSDTC overhead of System.Transactions.TransactionScope class.
     * It is important to note that the commitment of transactions in this transaction scope
     * abstraction is not actually atomic, meaning one of the abstracted transactions may be
     * committed before the database connection is interrupted and the committment of a second
     * transaction fails. VisionLink code already exhibited this behavior, so this class does 
     * not introduce any new fragility.
     *************************************************************************************/

    private readonly List<DbTransaction> _enrolledTransactions = new List<DbTransaction>();
    private bool _disposed;
    private readonly IsolationLevel? _level;

    public ObjectContextTransactionScope()
    {
    }

    public ObjectContextTransactionScope(IsolationLevel level)
    {
      _level = level;
    }

    public IDisposable EnrollObjectContexts(params object[] objectContexts)
    {
      if(null == objectContexts)
      {
        throw new ArgumentNullException("objectContexts");
      }
      if(_disposed)
      {
        throw new ObjectDisposedException(String.Format("{0} has been disposed", typeof(ObjectContextTransactionScope).Name));
      }

      foreach (var ctx in objectContexts.Cast<ObjectContext>())
      {
        if(ctx.Connection.State == ConnectionState.Closed)
        {
          ctx.Connection.Open();
        }
        if (_level.HasValue)
          _enrolledTransactions.Add(ctx.Connection.BeginTransaction(_level.Value));
        else
          _enrolledTransactions.Add(ctx.Connection.BeginTransaction());
      }
      _enrolledTransactions.Reverse();

      return this;
    }

    public void Commit()
    {
      if (_disposed)
      {
        throw new ObjectDisposedException(String.Format("{0} has been disposed", typeof(ObjectContextTransactionScope).Name));
      }
      foreach (var transaction in _enrolledTransactions)
      {
        // Per msdn, the following call disposes of transaction.Connection
        transaction.Commit();
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      // Check to see if Dispose has already been called. 
      if (!_disposed)
      {
        // If disposing equals true, dispose all managed 
        // and unmanaged resources. 
        if (disposing)
        {
          // Dispose managed resources.
          foreach (var transaction in _enrolledTransactions)
          {
            var connection = transaction.Connection;
            transaction.Dispose();
            if(connection != null && connection.State != ConnectionState.Closed)
            {
              connection.Close();
            }
          }
        }
        // Note disposing has been done.
        _disposed = true;
      }
    }

    ~ObjectContextTransactionScope()
    {
      Dispose(false);
    }
  }
}