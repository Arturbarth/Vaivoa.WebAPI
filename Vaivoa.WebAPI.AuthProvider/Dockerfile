#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Vaivoa.WebAPI.AuthProvider/Vaivoa.WebAPI.AuthProvider.csproj", "Vaivoa.WebAPI.AuthProvider/"]
COPY ["Vaivoa.WebAPI.Seguranca/Vaivoa.WebAPI.Seguranca.csproj", "Vaivoa.WebAPI.Seguranca/"]
COPY ["Vaivoa.WebAPI.DAL.Usuarios/Vaivoa.WebAPI.DAL.Usuarios.csproj", "Vaivoa.WebAPI.DAL.Usuarios/"]
RUN dotnet restore "Vaivoa.WebAPI.AuthProvider/Vaivoa.WebAPI.AuthProvider.csproj"
COPY . .
WORKDIR "/src/Vaivoa.WebAPI.AuthProvider"
RUN dotnet build "Vaivoa.WebAPI.AuthProvider.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vaivoa.WebAPI.AuthProvider.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vaivoa.WebAPI.AuthProvider.dll"]