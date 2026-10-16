FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY DesafioBenjamin/DesafioBenjamin.csproj DesafioBenjamin/
RUN dotnet restore DesafioBenjamin/DesafioBenjamin.csproj
COPY DesafioBenjamin/ DesafioBenjamin/
RUN dotnet publish DesafioBenjamin/DesafioBenjamin.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends tesseract-ocr tesseract-ocr-por tesseract-ocr-eng ca-certificates && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "DesafioBenjamin.dll"]
