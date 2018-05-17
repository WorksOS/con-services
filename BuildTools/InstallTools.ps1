Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
choco install kubernetes-helm
choco install kubernetes-cli
mkdir $HOME\Documents\.kube\
cp config $HOME\Documents\.kube\config