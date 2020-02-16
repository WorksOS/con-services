set $serviceStatus = & sc query _NHDeviceCapabilitySvc | find "RUNNING"

echo $serviceStatus

SLEEP 1000