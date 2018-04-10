param (
  [Parameter(Mandatory=$true)][string]$IP
)

$sleepSeconds = 10
$retrySeconds = 0

do {
  Write-Host "Test if Raptor is available on $IP..."
  Start-Sleep -seconds $sleepSeconds

  $retrySeconds += $sleepSeconds

  if ($retrySeconds -gt 90) {
    Exit -1
  }

} until(Test-NetConnection $IP -Port 80 | Where-Object { $_.TcpTestSucceeded } )
