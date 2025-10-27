FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /App

COPY . ./

RUN dotnet restore

RUN dotnet publish -o out

FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /App
COPY --from=build /App/out .

USER app

CMD ["dotnet", "DotNet.Docker.dll"]
