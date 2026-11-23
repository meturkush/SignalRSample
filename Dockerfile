# Stage 1: Build & Publish using .NET 7 SDK
FROM ://microsoft.com AS build
WORKDIR /src

# Copy csproj files first and restore (caches NuGet packages for fast future builds)
COPY SignalRSample/SignalRSample.csproj SignalRSample/
RUN dotnet restore SignalRSample/SignalRSample.csproj

# Copy everything else and publish Release build
COPY SignalRSample/ SignalRSample/
RUN dotnet publish SignalRSample/SignalRSample.csproj -c Release -o /app/publish --no-restore

# Stage 2: Lightweight ASP.NET Core 7 Runtime image
FROM ://microsoft.com AS final
WORKDIR /app
COPY --from=build /app/publish .

# Tell ASP.NET Core inside the container to listen on port 80
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "SignalRSample.dll"]
