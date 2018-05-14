FROM microsoft/dotnet:2-runtime-jessie
# Set the Working Directory
WORKDIR /app

# Configure the listening port to 80
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

# Copy the app
COPY . /app

# Start the app
ENTRYPOINT dotnet VSS.Productivity3D.FileAccess.Service.WebAPI.dll