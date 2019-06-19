
# https://www.speedoflightmedia.com/blog/aws-iam-roles-windows-containers/
# We need a route to the AWS Metadata Service
# This is only true for Windows containers

# Find out default Gateway
$gateway = (Get-NetRoute | Where { $_.DestinationPrefix -eq '0.0.0.0/0' } | Sort-Object RouteMetric | Select NextHop).NextHop

# Find out the interface used to connect to the internet, assuming we are running inside a windows container
$ifIndex = (Get-NetAdapter -InterfaceDescription "Hyper-V Virtual Ethernet*" | Sort-Object | Select -first 1 ifIndex).ifIndex

# Add a route to the AWS Infrastructure
New-NetRoute -DestinationPrefix 169.254.169.254/32 -InterfaceIndex $ifIndex -NextHop $gateway
Write-Host "Added route from"  $gateway "to AWS via Interface Index:" $ifIndex