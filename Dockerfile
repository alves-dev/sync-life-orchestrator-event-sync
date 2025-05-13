# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copia os arquivos e restaura as dependências
COPY . .
RUN dotnet publish -c Release -o /app/publish \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeAllContentForSelfExtract=true \
    -r linux-musl-x64

# Etapa final (imagem mínima apenas com runtime)
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final
WORKDIR /app

# Copia arquivos publicados
COPY --from=build /app/publish .

# Define o fuso horário (opcional)
ENV TZ=America/Sao_Paulo
RUN apk add --no-cache tzdata \
    && cp /usr/share/zoneinfo/$TZ /etc/localtime \
    && echo $TZ > /etc/timezone

ENTRYPOINT ["./event-sync"]