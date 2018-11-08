FROM microsoft/windowsservercore:10.0.14393.2068
RUN powershell Invoke-WebRequest -Uri "https://download.microsoft.com/download/9/E/6/9E63300C-0941-4B45-A0EC-0008F96DD480/NDP471-KB4033342-x86-x64-AllOS-ENU.exe" -OutFile dotnet-framework-installer.exe 
RUN .\dotnet-framework-installer.exe /q 
#RUN del .\dotnet-framework-installer.exe 
ENV COMPLUS_NGenProtectedProcess_FeatureEnabled=0 
RUN \Windows\Microsoft.NET\Framework64\v4.0.30319\ngen update 
RUN \Windows\Microsoft.NET\Framework\v4.0.30319\ngen update

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]
RUN set-itemproperty -path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord
RUN Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
RUN choco install dotnetcore-runtime -y

# Set the Working Directory
WORKDIR /app

# Configure the listening port to 80
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

# Copy the app
COPY . /app

# Create the mount point to hold volume from host for logs
#VOLUME logs

# Start the app
ENTRYPOINT ["powershell","-executionpolicy","bypass", ".\\SetupWebApi.ps1"]
