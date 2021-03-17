#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal AS base

# création de variable d'environnement utilis�es durant l'installation
ENV TZ=Europe/Kiev
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

RUN apt-get update && apt-get install -qq -y svxlink-server procps network-manager && apt-get clean 

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
WORKDIR /src
COPY ["SvxlinkManager.Dashboard/SvxlinkManager.Dashboard.csproj", "SvxlinkManager.Dashboard/"]
RUN dotnet restore "SvxlinkManager.Dashboard/SvxlinkManager.Dashboard.csproj"
COPY . .
WORKDIR "/src/SvxlinkManager.Dashboard"
RUN dotnet build "SvxlinkManager.Dashboard.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SvxlinkManager.Dashboard.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod 777 /app/SvxlinkConfig/svxlink.conf
ENTRYPOINT ["dotnet", "SvxlinkManager.Dashboard.dll"]