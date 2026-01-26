FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY KinkLinkCommon/KinkLinkCommon.csproj KinkLinkCommon/
COPY KinkLinkBot/KinkLinkBot.csproj KinkLinkBot/
RUN dotnet restore KinkLinkBot/KinkLinkBot.csproj
COPY . .
WORKDIR /src/KinkLinkBot
RUN dotnet publish "KinkLinkBot.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KinkLinkBot.dll"]
