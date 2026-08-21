FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
# cache bust: 2026-08-21
COPY KarateFinal/*.csproj ./KarateFinal/
RUN dotnet restore ./KarateFinal/KarateFinal.csproj
COPY KarateFinal/. ./KarateFinal/
RUN dotnet publish ./KarateFinal/KarateFinal.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "KarateFinal.dll"]
