FROM microsoft/windowsservercore:latest

# Set the Working Directory
WORKDIR /app

# Configure the listening port to 80
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

# Copy the app
COPY . /app
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]
RUN set-itemproperty -path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord
RUN Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
RUN choco install dotnet4.7.1
RUN choco install dotnetcore-runtime


# Create the mount point to hold volume from host for logs
#VOLUME logs

# Start the app
ENTRYPOINT ["powershell","-executionpolicy","bypass", ".\\SetupWebApi.ps1"]
