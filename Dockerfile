#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Converter.ServiceHost/Converter.ServiceHost.csproj", "Converter.ServiceHost/"]
COPY ["Converter.Service/Converter.Service.csproj", "Converter.Service/"]
RUN dotnet restore "Converter.ServiceHost/Converter.ServiceHost.csproj"
COPY . .
WORKDIR "/src/Converter.ServiceHost"
RUN dotnet build "Converter.ServiceHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Converter.ServiceHost.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://*:${PORT}
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Converter.ServiceHost.dll"]