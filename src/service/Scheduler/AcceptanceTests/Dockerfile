FROM microsoft/dotnet:2.0-sdk
COPY . /app
WORKDIR /app

# Remove CR (Carrage Return '\r') in sh files
RUN sed -i 's/\r$//' rm_cr.sh
RUN chmod 775 rm_cr.sh
RUN ["/bin/bash", "rm_cr.sh"]

# Create the mount point to hold volume from host for test results
VOLUME testresults

ENTRYPOINT ["/bin/bash", "runtests.sh"]
