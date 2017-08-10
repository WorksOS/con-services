FROM microsoft/windowsservercore:latest

SHELL ["powershell"]

WORKDIR /app

RUN iwr https://chocolatey.org/install.ps1 -UseBasicParsing | iex 
RUN choco install flyway.commandline -y

ENTRYPOINT ["flyway"]
CMD ["--help"]
