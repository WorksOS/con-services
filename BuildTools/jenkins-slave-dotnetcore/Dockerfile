FROM microsoft/dotnet:2.1-sdk-alpine

RUN apk add --no-cache git
RUN dotnet help

COPY .netrc /root/
RUN chmod 666 /root/.netrc
ENV HOME /root

WORKDIR /root