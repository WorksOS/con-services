FROM microsoft/windowsservercore:10.0.14393.2068
RUN powershell Invoke-WebRequest -Uri "http://go.microsoft.com/fwlink/?linkid=863265" -OutFile dotnet-472-framework-installer.exe 
RUN .\dotnet-472-framework-installer.exe /q 
ENV COMPLUS_NGenProtectedProcess_FeatureEnabled=0 
RUN \Windows\Microsoft.NET\Framework64\v4.0.30319\ngen update 
RUN \Windows\Microsoft.NET\Framework\v4.0.30319\ngen update

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]
RUN set-itemproperty -path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord
RUN Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
RUN choco install dotnetcore-runtime -y

# Set the Working Directory
WORKDIR /app

# Configure the listening port to 80
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

# Copy the app
COPY . /app

# Create the mount point to hold volume from host for logs
#VOLUME logs

# Start the app
ENTRYPOINT ["powershell","-executionpolicy","bypass", ".\\SetupWebApi.ps1"]
