using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.Transactions;
using VSS.UnitTest.Common._Framework.CustomAttributes.Implementation;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common._Framework.CustomAttributes
{
  public class DatabaseTestAspect : TestAspect<DatabaseTestAttribute>
  {
    [DebuggerStepThrough]
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
    public override IMessage SyncProcessMessage(IMessage msg)
    {
      if(msg == null)
        throw new ArgumentNullException("msg");

      DatabaseTestAttribute testTransactionAttribute = GetAttribute(msg);

      if (testTransactionAttribute == null) 
        return NextSink.SyncProcessMessage(msg);

      //***************************************************
      //****** USE DATABASE CONTEXT CREATION METHODS ******
      //***************************************************
      Debug.WriteLine("Database Context Creation Methods Implemented");
      ObjectContextFactory.ContextCreationFuncs((bool readOnly) => new NH_OP(ObjectContextFactory.ConnectionString("NH_OP"), readOnly));
      
      IMessage returnMethod;

      using (new TransactionScope(testTransactionAttribute.TransactionScopeOption, testTransactionAttribute.TransactionOptions))
      {
        Debug.WriteLine(string.Format("Transaction started for {0}.{1}", Type.Name, MethodName));

        returnMethod = NextSink.SyncProcessMessage(msg);
      }

      Debug.WriteLine(string.Format("Transaction rolled back for {0}.{1}", Type.Name, MethodName));

      return returnMethod;
    }
  }
}
