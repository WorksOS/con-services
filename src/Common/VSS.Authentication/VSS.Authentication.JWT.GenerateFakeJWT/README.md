# Fake TPaaS JWT generator

Generates a fake JWT

## Usage - command line

- Install .NET Core SDK
- Open terminal
- `cd` to this folder
- run `dotnet run USER_UID_HERE`

Example output

```
X-JWT-Assertion: xxxx.eyJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IlZMMi4wIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91c2VydHlwZSI6IkFQUExJQ0FUSU9OX1VTRVIiLCJodHRwOi8vd3NvMi5vcmcv
Y2xhaW1zL3V1aWQiOiJjYjBlYmU4YS1iOTYwLTRmNWQtODU1Mi0wNDM5ZjVmMGRmZTYifQ==.xxxx     
```

## Usage - docker

You can also run as a docker container.
**TODO: create a docker-compose file to simplify this**

```
cd VSS.Authentication
docker build -t generate-fake-jwt -f Dockerfile-GenerateFakeJWT .
docker run -it --rm generate-fake-jwt USER_UID_HERE
```
