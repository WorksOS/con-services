FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.1-sdk as builder
ARG SERVICE_PATH

COPY . /build/
WORKDIR /build

RUN dotnet publish ./$SERVICE_PATH/src/WebApi/VSS.Productivity3D.WebApi.csproj -o /artifacts -f netcoreapp2.1 --runtime linux-x64

FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-2.1-runtime

WORKDIR /app

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

RUN apt-get update \
&& apt-get install --assume-yes \
   libc6-dev \
   wget

RUN wget https://download.mono-project.com/repo/ubuntu/pool/main/libg/libgdiplus/libgdiplus_5.6-0xamarin3+debian9b1_amd64.deb && \
    apt-get install -y ./libgdiplus_5.6-0xamarin3+debian9b1_amd64.deb

COPY --from=builder /artifacts/ .

RUN ls -la /app

ENTRYPOINT ["dotnet", "VSS.Productivity3D.WebApi.dll"]
