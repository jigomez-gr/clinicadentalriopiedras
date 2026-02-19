# Etapa 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos los archivos de proyecto (.csproj) primero para aprovechar la caché de Docker
COPY ["ClinicaWeb/ClinicaWeb.csproj", "ClinicaWeb/"]
COPY ["ClinicaData/ClinicaData.csproj", "ClinicaData/"]
COPY ["ClinicaEntidades/ClinicaEntidades.csproj", "ClinicaEntidades/"]

# Restauramos las dependencias
RUN dotnet restore "ClinicaWeb/ClinicaWeb.csproj"

# Copiamos todo el código fuente y compilamos
COPY . .
WORKDIR "/src/ClinicaWeb"
RUN dotnet build "ClinicaWeb.csproj" -c Release -o /app/build

# Etapa 2: Publicación
FROM build AS publish
RUN dotnet publish "ClinicaWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 3: Ejecución (Final)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
# --- AÑADE ESTAS DOS LÍNEAS AQUÍ ---
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
# ----------------------------------

# Configuramos el puerto interno para que .NET escuche en el 4431
ENV ASPNETCORE_URLS=http://+:4431
EXPOSE 4431

# Comando de inicio
ENTRYPOINT ["dotnet", "ClinicaWeb.dll"]