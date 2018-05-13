FROM microsoft/dotnet:2.1.300-rc1-sdk-alpine3.7
COPY . /build/
RUN dotnet build /build/TRex.Framework.netstandard.sln --output /artifacts