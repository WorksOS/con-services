param (
  [Parameter(Mandatory=$true)][string]$IP
)

do {
  Write-Host "Test if Raptor is available..."
  sleep 10      
} until(Test-NetConnection $IP -Port 80 | ? { $_.TcpTestSucceeded } )
