using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  public class AppServiceInstaller
  {
    public enum ActionType
    {
      None,
      Restart,
      Reboot,
      RunCommand
    }

    public static bool ServiceDescription( string serviceName, string description, RecoveryAction[] recoveryActions )
    {
      IntPtr ptr1 = IntPtr.Zero;
      IntPtr ptr2 = IntPtr.Zero;

      try
      {
        ptr1 = OpenSCManager( null, null, 0xf003f );
        if ( ptr1 == IntPtr.Zero )
        {
          return false;
        }
        ptr2 = OpenService( ptr1, serviceName, 0xf01ff );
        if ( ptr2 == IntPtr.Zero )
        {
          return false;
        }
        if ( !ChangeServiceConfig2( ptr2, description ) )
        {
          return false;
        }
        if ( !ChangeServiceConfig2( ptr2, recoveryActions ) )
        {
          return false;
        }
      }
      finally
      {
        if ( ptr2 != IntPtr.Zero )
        {
          CloseServiceHandle( ptr2 );
        }
        if ( ptr1 != IntPtr.Zero )
        {
          CloseServiceHandle( ptr1 );
        }
      }
      return true;
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct RecoveryAction
    {
      public ActionType Action;
      public int DelayMs;
    }

    [StructLayout( LayoutKind.Sequential )]
    private struct LUID
    {
      public int LowPart;
      public int HighPart;
    }

    [StructLayout( LayoutKind.Sequential )]
    private struct LUID_AND_ATTRIBUTES
    {
      public LUID Luid;
      public int Attributes;
    }

    [StructLayout( LayoutKind.Sequential )]
    private struct TOKEN_PRIVILEGES
    {
      public int PrivilegeCount;
      public LUID_AND_ATTRIBUTES Privileges;
    }

    [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
    private struct SERVICE_DESCRIPTION
    {
      public string lpDescription;
    }

    [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
    private struct SERVICE_FAILURE_ACTIONS
    {
      public int dwResetPeriod;
      public string lpRebootMsg;
      public string lpCommand;
      public int cActions;
      public IntPtr lpsaActions;
    }


    [DllImport( "Kernel32.dll", CharSet = CharSet.Unicode )]
    private static extern IntPtr GetCurrentProcess();

    [DllImport( "Advapi32.dll", EntryPoint = "OpenSCManagerW", CharSet = CharSet.Unicode )]
    private static extern IntPtr OpenSCManager( string lpMachineName, string lpDatabaseName, int dwDesiredAccess );

    [DllImport( "Advapi32.dll", EntryPoint = "OpenServiceW", CharSet = CharSet.Unicode )]
    private static extern IntPtr OpenService( IntPtr hSCManager, string lpServiceName, int dwDesiredAccess );

    [DllImport( "Advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true )]
    private static extern bool OpenProcessToken( IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle );

    [DllImport( "Advapi32.dll", EntryPoint = "LookupPrivilegeValueW", CharSet = CharSet.Unicode )]
    private static extern bool LookupPrivilegeValue( string lpSystemName, string lpName, ref LUID lpLuid );

    [DllImport( "Advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true )]
    private static extern bool AdjustTokenPrivileges( IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, int BufferLength, IntPtr PreviousState, IntPtr ReturnLength );
    
    [DllImport( "Advapi32.dll", EntryPoint = "ChangeServiceConfig2W", CharSet = CharSet.Unicode, SetLastError = true )]
    private static extern bool ChangeServiceConfig2SD( IntPtr hService, int dwInfoLevel, ref SERVICE_DESCRIPTION lpInfo );

    [DllImport( "Advapi32.dll", EntryPoint = "ChangeServiceConfig2W", CharSet = CharSet.Unicode, SetLastError = true )]
    private static extern bool ChangeServiceConfig2SFA( IntPtr hService, int dwInfoLevel, ref SERVICE_FAILURE_ACTIONS lpInfo );

    [DllImport( "Advapi32.dll", CharSet = CharSet.Unicode )]
    private static extern bool CloseServiceHandle( IntPtr hSCObject );

    private static bool ChangeServiceConfig2( IntPtr hService, string description )
    {
      if ( ( description != null ) && ( description.Length > 0 ) )
      {
        SERVICE_DESCRIPTION serviceDescription;
        serviceDescription.lpDescription = description;

        return ChangeServiceConfig2SD( hService, 1, ref serviceDescription );
      }
      return false;
    }

    private static bool ChangeServiceConfig2( IntPtr hService, RecoveryAction[] recoveryActions )
    {
      bool result = false;
      RecoveryAction[] actionArray = recoveryActions;

      if ( actionArray == null )
      {
        actionArray = new RecoveryAction[3];

        for ( int i = 0; i < 3; i++ )
        {
          actionArray[i].Action = ActionType.Restart;
          actionArray[i].DelayMs = 60000;
        }
      }
      GCHandle handle1 = GCHandle.Alloc( actionArray, GCHandleType.Pinned );

      try
      {
        SERVICE_FAILURE_ACTIONS failureActions;
        failureActions.cActions = Math.Min( actionArray.Length, 3 );
        failureActions.dwResetPeriod = 3600;
        failureActions.lpCommand = "";
        failureActions.lpRebootMsg = "";
        failureActions.lpsaActions = handle1.AddrOfPinnedObject();

        result = ChangeServiceConfig2SFA( hService, 2, ref failureActions );
      }
      finally
      {
        handle1.Free();
      }
      return result;
    }
  }
}
