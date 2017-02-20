FROM microsoft/dotnet:1.1-runtime

# Copy the app
COPY . /app

# Set the Working Directory
WORKDIR /app

RUN chmod 775 wait-for-it.sh

# Create the mount point to hold volume from host for logs
VOLUME logs

# Start the app
ENTRYPOINT ["/bin/bash", "runme.sh"]
