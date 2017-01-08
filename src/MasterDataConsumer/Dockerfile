FROM microsoft/dotnet:1.1-runtime

# Set the Working Directory
WORKDIR /app

# Copy the app
COPY . /app

# Create the mount point to hold volume from host for logs
VOLUME logs

# Start the app
ENTRYPOINT ["/bin/bash", "runme.sh"]
