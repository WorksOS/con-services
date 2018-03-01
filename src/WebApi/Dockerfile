#FROM microsoft/windowsservercore:latest
FROM microsoft/dotnet-framework:4.7.1-windowsservercore-1709

# Set the Working Directory
WORKDIR /app

# Configure the listening port to 80
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

# Copy the app
COPY . /app
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]
RUN set-itemproperty -path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord

# Create the mount point to hold volume from host for logs
#VOLUME logs

# Start the app
ENTRYPOINT ["powershell","-executionpolicy","bypass", ".\\SetupWebApi.ps1"]
