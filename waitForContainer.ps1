param (
  [Parameter(Mandatory=$true)][string]$containerIPAddress
)

Write-Host "Attempting connection to 3DP Raptor service..." -ForegroundColor DarkGray

$sleepSeconds = 10
$retryAttempts = 0

while ($retryAttempts -ne 5) {
  Write-Host "Test service connection on $containerIPAddress..."

  if (Test-NetConnection $containerIPAddress -Port 80 | Where-Object { $_.TcpTestSucceeded })
  {
    Exit 0
    Write-Host "Connection succeeded." -ForegroundColor DarkGray
  }

  $retryAttempts++
  Start-Sleep -seconds $sleepSeconds
}

Write-Host "Failed to connect after $retryAttempts attempts." -ForegroundColor DarkRed
Exit -1