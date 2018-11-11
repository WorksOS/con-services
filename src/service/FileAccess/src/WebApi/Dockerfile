FROM registry-docker-registry.jenkins:5000/dotnet:2.0-runtime
# Set the Working Directory
WORKDIR /app

# Configure the listening port to 80
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

# Copy the app
COPY . /app

# Start the app
ENTRYPOINT dotnet VSS.Productivity3D.FileAccess.WebAPI.dll
