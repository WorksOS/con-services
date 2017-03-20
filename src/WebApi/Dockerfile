#FROM microsoft/dotnet:nanoserver
#FROM microsoft/nanoserver
FROM microsoft/windowsservercore

# Set the Working Directory
WORKDIR /app

# Configure the listening port to 80
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

# Copy the app
COPY . /app

# Create the mount point to hold volume from host for logs
#VOLUME logs

ADD SetupWebAPI.ps1 /app/SetupWebAPI.ps1
RUN powershell.exe -executionpolicy bypass /app/SetupWebAPI.ps1

# Start the app
ENTRYPOINT WebApi.exe
