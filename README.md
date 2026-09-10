# GameZone Mobile

**Stable free public host (1–2 years):** Oracle Always Free — see [`../deploy/oracle/README.md`](../deploy/oracle/README.md). Do not use Cloudflare Quick Tunnels for production.

Separate Android + PostgreSQL solution. The Windows WPF app in the parent folder is unchanged and still uses its local SQLite file.

```
You / friend (MAUI phone)
        │
        ▼
  GameZone.Api  ──►  PostgreSQL (Supabase, Neon, or Docker)
```

## 1. Create the online database

**Option A — Supabase (free, recommended)**

1. Create a project at [https://supabase.com](https://supabase.com)
2. Project Settings → Database → Connection string (URI)
3. Put it in `src/GameZone.Api/appsettings.json` as `ConnectionStrings:Postgres`

Example:

```
Host=db.xxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

**Option B — local Docker**

```powershell
cd mobile
docker compose up -d
```

Uses `Host=localhost;Port=5432;Database=gamezone;Username=gamezone;Password=gamezone`

## 2. Run the API

```powershell
cd mobile
dotnet run --project src\GameZone.Api\GameZone.Api.csproj
```

API listens on `http://0.0.0.0:5088` (Swagger at `/swagger`).

First start creates tables and seeds:

- Login: `admin` / `admin123`
- Items: PS5, VR, Steering Wheel Racing, Gaming PC

## 3. Install on your phone (web app)

The phone app you have been using is the API website. You can add it to the home screen today — an Android APK needs the MAUI workload, which is not installed on this PC yet.

**Same Wi‑Fi as this computer**

1. Keep the API running
2. On the phone, open Chrome and go to `http://YOUR-PC-LAN-IP:5088`
3. Chrome menu → **Add to Home screen** / **Install app**

Find your PC IP with `ipconfig` (IPv4). Allow port **5088** in Windows Firewall.

**Away from home (online database)**

1. Create a free Postgres database at [Supabase](https://supabase.com) or [Neon](https://neon.tech)
2. Put the connection string in `src/GameZone.Api/appsettings.json` (`ConnectionStrings:Postgres`) or set `DATABASE_URL`
3. Host `GameZone.Api` on Render / Railway / a VPS with that same connection string
4. On the phone, open the public HTTPS URL and add it to the home screen

The phone never talks to Postgres directly. It talks to the API; the API talks to the online database.

## 4. Install the MAUI Android workload (optional APK)

Open **Developer PowerShell as Administrator**:

```powershell
dotnet workload install maui-android
```

Then build and deploy the phone app:

```powershell
cd mobile
dotnet build src\GameZone.Maui\GameZone.Maui.csproj -f net8.0-android
```

In Visual Studio 2022: open `mobile\GameZone.Mobile.sln`, set **GameZone.Maui** as startup, pick an emulator or USB phone.

For a native APK, point the MAUI login **API URL** at the same host: emulator `http://10.0.2.2:5088`, real phone `http://YOUR-PC-LAN-IP:5088` or your public HTTPS URL.

## Host the API so both of you can use it away from home

Deploy `GameZone.Api` to any small host (Render, Railway, a VPS) and put the public HTTPS URL in the phone Settings. Keep the Postgres connection string only on the server — never in the phone app.

## Login

`admin` / `admin123`

Create an Employee user for your friend from **Users**.
