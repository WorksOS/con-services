FROM mcr.microsoft.com/dotnet/framework/runtime:4.7.2-windowsservercore-ltsc2019

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
