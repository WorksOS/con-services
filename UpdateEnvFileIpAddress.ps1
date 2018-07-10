# Get the ip address from the network adaptor 
$ipV4 = Get-NetAdapter | ? name -eq ‘Ethernet’ | Get-NetIPAddress -ErrorAction 0 | ? PrefixOrigin -eq ‘Dhcp’ | Select -ExpandProperty IPV4Address
$ipV4

(Get-Content docker-compose-local.env) | Foreach-Object {$_ -replace "LOCALIPADDRESS", $ipV4} | Set-Content docker-compose-local.env
(Get-Content SetDockerEnvironmentVariables.ps1) | Foreach-Object {$_ -replace "LOCALIPADDRESS", $ipV4} | Set-Content SetDockerEnvironmentVariables.ps1