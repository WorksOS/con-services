param (
  [Parameter(Mandatory=$true)][string]$containerIPAddress
)

Write-Host "Attempting connection to 3DP Raptor service..." -ForegroundColor DarkGray

$sleepSeconds = 5
$retryCounter = 0
$portNumber = 80

while ($retryCounter -ne 5) {
  Write-Host "Test service connection on $containerIPAddress::$portNumber..."

  if (Test-NetConnection $containerIPAddress -Port $portNumber | Where-Object { $_.TcpTestSucceeded })
  {
    Write-Host "Connection succeeded." -ForegroundColor DarkGray
    Exit 0
  }

  $retryCounter++
  Start-Sleep -seconds $sleepSeconds
}

Write-Host "Failed to connect after $retryCounter attempts." -ForegroundColor DarkRed
Exit -1