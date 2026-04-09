# KMFlow.Api

API backend KMFlow berbasis ASP.NET Core (.NET 8) dengan EF Core (SQL Server), JWT Authentication, dan Swagger.

## Prasyarat

- .NET SDK 8.x
- SQL Server (LocalDB/Express/Developer/Standard)

## Konfigurasi

Konfigurasi utama ada di [appsettings.json](file:///c:/Users/Nanta/Desktop/TOTALBP-TEST/KMFlow.Api/KMFlow.Api/appsettings.json):

- `ConnectionStrings:DefaultConnection` (SQL Server)
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiresMinutes`

Untuk konfigurasi lokal tanpa ikut ter-commit, buat file `KMFlow.Api/appsettings.Local.json` lalu jalankan dengan:

```bash
set ASPNETCORE_ENVIRONMENT=Local
dotnet run --project .\KMFlow.Api\KMFlow.Api.csproj
```

## Database (EF Core)

Migrations berada di project `KMFlow.Infrastucture`. Untuk membuat/update database:

```bash
dotnet ef database update --project .\KMFlow.Infrastucture\KMFlow.Infrastucture.csproj --startup-project .\KMFlow.Api\KMFlow.Api.csproj
```

## Menjalankan API

```bash
dotnet restore
dotnet run --project .\KMFlow.Api\KMFlow.Api.csproj
```

Saat environment `Development`, Swagger otomatis aktif:

- https://localhost:44365/swagger

## Catatan

- CORS policy bernama `Frontend` mengizinkan origin `http(s)://localhost:5005` dan `http(s)://127.0.0.1:5005` (lihat [Program.cs](file:///c:/Users/Nanta/Desktop/TOTALBP-TEST/KMFlow.Api/KMFlow.Api/Program.cs)).
