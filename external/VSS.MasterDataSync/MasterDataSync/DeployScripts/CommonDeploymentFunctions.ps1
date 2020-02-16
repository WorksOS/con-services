Function ServiceExists([string] $ServiceName) {
    [bool] $Return = $False
    if ( Get-WmiObject -Class Win32_Service -Filter "Name='$ServiceName'" ) {
        $Return = $True
    }

		if ($Return -eq $true)
    {
        Write-host "ServiceExists. OK true"
    }
    else
    {
        Write-host "ServiceExists false"
    }
    Return $Return
}

function InstallService($ServiceName, $ServicePath)
{
  #Write-host ("InstallService called")
	$exists = ServiceExists($ServiceName) 
  If($exists -eq $False){
	 Write-host ("Installing Service ")
	 & $ServicePath install
	 Write-host ("Installed Service")		
  }

	Write-host ("InstallService return")
}

function UninstallService($ServiceName, $ServicePath)
{
  Write-host ("InstallService called")
	$exists = ServiceExists($ServiceName) 
  If($exists -eq $True){
	 StopServices @($ServiceName)
	 Write-host ("Uninstalling Service ")
	 & $ServicePath uninstall
	 Write-host ("Installed Service")		
  }

	Write-host ("InstallService return")
}

function StopServices($Services){
    foreach($serviceName in $Services){
        If(ServiceExists($serviceName)){
           If(Get-Service -Name $ServiceName | Where-Object {$_.status -eq "running"}){
                Write-host ("Stopping Service " + $serviceName)
                Stop-Service $ServiceName
                Write-host ("Stopped Service " + $serviceName)
           }
         }
        Else{
            Write-host ($serviceName  + "Not Exists")
        }
    }
}

function StartServices($Services){
    foreach($serviceName in $Services){
        If(ServiceExists($serviceName)){
           If(Get-Service -Name $ServiceName | Where-Object {$_.status -ne "running"}){
                Write-host ("Starting Service " + $serviceName)
                Start-Service $ServiceName
                Write-host ("Started Service " + $serviceName)
           }
         }
        Else{
            Write-host ($serviceName  + "Not Exists")
        }
    }
}

