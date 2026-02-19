# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/SherpaOnnxTts/SherpaOnnxTts.csproj", "SherpaOnnxTts/"]

RUN dotnet restore "SherpaOnnxTts/SherpaOnnxTts.csproj"

COPY src/ .

WORKDIR "/src/SherpaOnnxTts"

RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN groupadd -r appuser && useradd -r -g appuser -d /app appuser

RUN apt-get update && apt-get install -y --no-install-recommends \
    libgomp1 \
    && rm -rf /var/lib/apt/lists/*

# 設定 Port 為 5364 (聖母峰基地營高度)
ENV ASPNETCORE_HTTP_PORTS=5364

COPY --from=build --chown=appuser:appuser /app/publish .

USER appuser

EXPOSE 5364

ENTRYPOINT ["dotnet", "CloudyWing.SherpaOnnxTts.dll"]