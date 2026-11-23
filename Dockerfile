FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY SignalRSample/SignalRSample.csproj SignalRSample/
RUN dotnet restore SignalRSample/SignalRSample.csproj

COPY SignalRSample/ SignalRSample/
RUN dotnet publish SignalRSample/SignalRSample.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "SignalRSample.dll"]
