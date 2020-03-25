FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-sdk

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
