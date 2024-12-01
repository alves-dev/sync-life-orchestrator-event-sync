FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG TARGETARCH
ARG BUILDPLATFORM

WORKDIR /app

# Copia os arquivos do projeto e restaura as dependências
COPY . ./
RUN dotnet restore

# Compila a aplicação
# RUN dotnet publish -c Release -o out
RUN dotnet build -c Release -o out -a $TARGETARCH


FROM build AS publish
RUN dotnet publish -c Release -o out \
    #--runtime alpine-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true \
    -a $TARGETARCH


FROM --platform=$BUILDPLATFORM base AS final
ARG TARGETARCH
ARG BUILDPLATFORM

# Copia os binários da etapa de construção
COPY --from=publish /app/out .

# Configura o timezone para America/Sao_Paulo
# RUN apt-get update && apt-get install -y tzdata
# ENV TZ=America/Sao_Paulo
# RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
# RUN apt-get clean && rm -rf /var/lib/apt/lists/*

WORKDIR /app
# Define o comando de entrada
ENTRYPOINT ["./event-sync"]
