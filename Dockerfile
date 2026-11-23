# Stage 1: Build & Publish using .NET 7 SDK
FROM ://microsoft.com AS build
WORKDIR /src

# Restore dependencies first for fast layer caching
COPY SignalRSample/SignalRSample.csproj SignalRSample/
RUN dotnet restore SignalRSample/SignalRSample.csproj

# Copy source code and publish Release build
COPY SignalRSample/ SignalRSample/
RUN dotnet publish SignalRSample/SignalRSample.csproj -c Release -o /app/publish --no-restore

# Stage 2: Lightweight ASP.NET Core 7 Runtime image
FROM ://microsoft.com AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "SignalRSample.dll"]
