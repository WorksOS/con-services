function Login-Aws {
    Write-Host "`n##[section]Authenticating with AWS ECR..." -ForegroundColor Green

    if ((Test-Path env:AWS_PROFILE)) {
        Write-Host "Found AWS_PROFILE:$env:AWS_PROFILE, using '--profile $env:AWS_PROFILE'"
        $profileArg = "--profile $env:AWS_PROFILE"
    }

    Invoke-Expression "aws ecr get-login-password --region us-west-2 $profileArg | docker login --username AWS --password-stdin 940327799086.dkr.ecr.us-west-2.amazonaws.com"
}
