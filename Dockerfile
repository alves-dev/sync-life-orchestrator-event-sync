# Etapa 1: Construção
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia os arquivos do projeto e restaura as dependências
COPY . ./
RUN dotnet restore

# Compila a aplicação
RUN dotnet publish

# Etapa 2: Execução
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

# Copia os binários da etapa de construção
COPY --from=build /app/out .

# Configura o timezone para America/Sao_Paulo
RUN apt-get update && apt-get install -y tzdata
ENV TZ=America/Sao_Paulo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
RUN apt-get clean && rm -rf /var/lib/apt/lists/*

# Define o comando de entrada
ENTRYPOINT ["dotnet", "event-sync.dll"]
