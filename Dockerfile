FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
EXPOSE 80
EXPOSE 443
ARG NUGET_USERNAME
ARG NUGET_TOKEN
ARG version

COPY ["School.Exam.SIUSS.Services.Authentication/School.Exam.SIUSS.Services.Authentication.csproj", "School.Exam.SIUSS.Services.Authentication/"]
COPY ["NuGet.config", "School.Exam.SIUSS.Services.Authentication/"]

RUN dotnet restore "School.Exam.SIUSS.Services.Authentication/School.Exam.SIUSS.Services.Authentication.csproj" --configfile School.Exam.SIUSS.Services.Authentication/NuGet.config

COPY . .

RUN dotnet publish "School.Exam.SIUSS.Services.Authentication/School.Exam.SIUSS.Services.Authentication.csproj" -c Release -o out /p:Version=$version

FROM mcr.microsoft.com/dotnet/aspnet:8.0 
WORKDIR /app

COPY --from=build /src/out .
ENTRYPOINT ["dotnet", "School.Exam.SIUSS.Services.Authentication.dll"]
