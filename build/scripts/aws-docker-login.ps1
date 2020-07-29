function Login-Aws {
    Write-Host "`nAuthenticating with AWS ECR..." -ForegroundColor Green
    Write-Host "Determining AWS CLI version..."

    aws --version

    $awsVersion = (aws --version).Split(' ')[0].Split('/')[1].Split(' ')
    $versionMajorMinor = [decimal]($awsVersion[0].SubString(0, $awsVersion.LastIndexOf('.')))
    $canUseGetLoginPassword = $versionMajorMinor -ge 1.18

    if ($canUseGetLoginPassword) {
        # Azure pipelines use a recent version of AWS CLI that has replaced get-login with get-login-password.
        aws ecr get-login-password --region us-west-2 | docker login --username AWS --password-stdin 940327799086.dkr.ecr.us-west-2.amazonaws.com
        if (-not $?) { Exit-With-Code ([ReturnCode]::AWS_ECR_LOGIN_FAILED) }
    }
    else {
        # Retain backward compatibility for running locally on team development PCs with older AWS CLI installed.
        Write-Host "Found older version of AWS CLI, failing back to 'get-login'`n"
        Invoke-Expression -Command (aws ecr get-login --no-include-email --region us-west-2 --profile fsm-okta)
        if (-not $?) { Exit-With-Code ([ReturnCode]::AWS_ECR_LOGIN_FAILED) }
    }
}
