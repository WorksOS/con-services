ARG APP_BUILD_IMAGE=276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:dotnet-2.0-sdk

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
ENTRYPOINT dotnet VSS.Productivity3D.Filter.WebAPI.dll
