FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder
COPY /Sharpmaid /app
RUN dotnet publish -c Release -o /out /app/Sharpmaid.csproj

FROM mcr.microsoft.com/dotnet/runtime:6.0 as base
WORKDIR /app
COPY --from=builder /out .

ENTRYPOINT ["dotnet", "Sharpmaid.dll"]
