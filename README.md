# WOL Server

Lightweight .NET 9 API to send Wake-on-LAN magic packets.

## Features
- JWT auth (Bearer)
- Swagger UI for live test
- JSON source-gen for perf
- DI based services (IUdpSender, IWolService)
- Unit and integ tests (xUnit, WebAppFactory)

## Config
Use settings:
- JWT_KEY (secret)
- JWT_ISSUER (e.g. wol-server)
- JWT_AUDIENCE (e.g. wol-clients)

The app also reads `appsettings.json` for defaults.

## Run
```powershell
dotnet build
dotnet run --project WOL-Server
```
Swagger: http://localhost:5157/swagger

## Tests
```powershell
dotnet test
```

## Auth and use
- Get a JWT using your tool or dev token
- In Swagger click Authorize and paste: `Bearer {token}`
- POST /wol with body `{ "macAddress": "AA:BB:CC:DD:EE:FF" }`

Behavior:
- The `/wol` endpoint requires a valid Bearer JWT. Requests without a valid token return 401.
- On success the endpoint returns 200 OK with JSON `{ "success": true }`.
- If the request body is missing or the MAC address is invalid the endpoint returns 400 with `{ "success": false }`.
- The API validates the token signature using `JWT_KEY` and (optionally) `JWT_ISSUER`/`JWT_AUDIENCE` when configured.

## Tools

### WOL-TokenGen 

Use the `WOL-TokenGen` tool to generate a JWT that the API will accept (the repository also includes a small console project named `WOL-TokenGen`).

Basic usage examples (from repository root):

Run the published exe (recommended for Windows dev):

```powershell
# after building or publishing, run the exe; args: [subject] [minutes]
WOL-TokenGen.exe johndoe 30
```

Or run with dotnet (framework-dependent dll):

```powershell
dotnet run --project WOL-TokenGen -- johndoe 30
```

Flags supported by the tool:
- `--subject` / `-s` : subject claim (sub)
- `--mins` / `-m` : token lifetime in minutes
- `--show-key` : print the JWT key to stderr (use only when necessary)
- `--help` / `-h` : show help

Example usage with the API (Swagger or curl):

1) Generate a token:

```powershell
# prints a single-line JSON with the token
WOL-TokenGen.exe johndoe 60
# or: dotnet run --project WOL-TokenGen -- --subject diego --mins 60
```

2) Use in Swagger: click Authorize and paste `Bearer {token}`.

3) Use from curl (replace {token} and MAC):

```powershell
curl -X POST "http://localhost:5157/wol/" -H "Authorization: Bearer {token}" -H "Content-Type: application/json" -d '{"macAddress":"AA:BB:CC:DD:EE:FF"}'
```

Security note
- The `WOL-TokenGen` in this repo is a developer convenience. Do not commit production secrets.
- In production, provide `JWT_KEY` (and issuer/audience) via secure secret storage or environment variables.

### Windows Service version

You can run the same API as a Windows Service using the `WOL-WindowsService` project included in this repo.

1) Publish for Windows (example):

```powershell
dotnet publish .\WOL-WindowsService -c Release -r win-x64 --self-contained false
# published files will be under:
# .\WOL-WindowsService\bin\Release\net9.0\win-x64\publish\
```

2) Install the service (run as Administrator):

PowerShell (recommended):

```powershell
New-Service -Name "WOL-WindowsService" -BinaryPathName "C:\path\to\WOL-WindowsService.exe" -DisplayName "WOL Windows Service" -StartupType Automatic
```

Or using sc.exe:

```powershell
sc create WOL-WindowsService binPath= "C:\path\to\WOL-WindowsService.exe" start= auto
```

3) Start / Stop / Uninstall:

```powershell
Start-Service WOL-WindowsService
Stop-Service WOL-WindowsService
sc delete WOL-WindowsService
```

4) Environment configuration
- The service reads `JWT_KEY`, `JWT_ISSUER` and `JWT_AUDIENCE` from environment variables (or `appsettings.json` if present). Set machine-level env vars for the service, e.g.:

```powershell
setx /M JWT_KEY "your-secret-here"
setx /M JWT_ISSUER "wol-server"
setx /M JWT_AUDIENCE "wol-clients"
# then restart the service so it picks up the new machine env vars
```

For production, prefer using a secret store (Key Vault, Secret Manager) instead of machine env vars.

5) Firewall
If the service listens on a non-loopback port (default dev port 5157), allow the port in Windows Firewall:

```powershell
New-NetFirewallRule -DisplayName "WOL Server" -Direction Inbound -LocalPort 5157 -Protocol TCP -Action Allow
```

6) Self-contained publish (optional)
If you prefer a single self-contained executable (no .NET runtime required on target), publish as:

```powershell
dotnet publish .\WOL-WindowsService -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Notes
- `WOL-WindowsService` uses `Host.UseWindowsService()` and should behave like the `WOL-Server` API.
- If the referenced `WOL-Server` project enables AOT (`<PublishAot>true</PublishAot>`), test the publish output carefully: AOT and cross-project publish can affect publish time and produced artifacts.
- Check Windows Event Viewer or configure logging to capture runtime errors from the service.

## Notes
- Use Key Vault or env vars in prod.