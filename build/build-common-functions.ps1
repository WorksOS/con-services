function Exit-With-Code {
    param(
        [ReturnCode][Parameter(Mandatory = $true)]$code
    )

    TrackTime $timeStart

    if ($code -eq [ReturnCode]::SUCCESS) {
        Write-Host "`nExiting: $code" -ForegroundColor Green
    }
    else {
        Write-Host "`nExiting with error: $code" -ForegroundColor Red
    }

    Pop-Location
    Exit $code
}

function TrackTime($Time) {
    if (!($Time)) { 
        Return 
    }
    Else {
        $executionTime = ((Get-Date) - $Time)
        $executionMinutes = "{0:N2}" -f $executionTime.TotalMinutes
        Write-Host "Script completed in ${executionMinutes} minutes."
    }
}
