param (
  [Parameter(Mandatory=$true)][string]$IP
)

do {
  Write-Host "Test if Raptor is available on $IP..."
  Start-Sleep -seconds 10
} until(Test-NetConnection $IP -Port 80 | ? { $_.TcpTestSucceeded } )
