ARG APP_BUILD_IMAGE=registry-docker-registry.jenkins:5000/dotnet:2.1-sdk

FROM ${APP_BUILD_IMAGE}

# Set the Working Directory
WORKDIR /app

# Configure the listening port to 80
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

# Copy the app
COPY . /app

# Create the mount point to hold volume from host for logs
VOLUME logs

# Start the app
ENTRYPOINT dotnet VSS.MasterData.Project.WebAPI.dll
