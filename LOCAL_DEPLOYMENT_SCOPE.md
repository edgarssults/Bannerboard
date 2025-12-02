# Bannerboard Local Deployment - Scope of Work

## Executive Summary
Convert Bannerboard from Azure-hosted web UI to fully local deployment. The WebSocket server already runs locally in the Mount & Blade II game mod, only the Blazor WebAssembly UI needs local hosting configuration.

---

## Current Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Current Deployment                        │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Game Mod (Ed.Bannerboard)                                   │
│  ├─ .NET Framework 4.7.2                                     │
│  ├─ SuperSocket.WebSocket Server                             │
│  ├─ Listens on: ws://localhost:2020                          │
│  └─ Bannerlord Modules Directory                             │
│                                                               │
│                         ↕                                     │
│                   (WebSocket)                                 │
│                         ↕                                     │
│                                                               │
│  Blazor WASM UI (Ed.Bannerboard.UI)                          │
│  ├─ .NET 10.0                                                │
│  ├─ Hosted on: https://bannerboard.azurewebsites.net         │
│  ├─ Connects to: ws://localhost:2020                         │
│  └─ Runs in user's browser                                   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### Key Findings
- **WebSocket Port**: Hardcoded to `2020` in both server (`Ed.Bannerboard\Init.cs:73`) and client (`Index.razor.cs:63`)
- **Connection Pattern**: Client connects from browser to `ws://localhost:2020` (already local)
- **Azure Dependency**: Only for serving static Blazor WASM files (HTML, CSS, JS, WebAssembly DLLs)
- **No Server-Side Logic**: Blazor WebAssembly runs entirely in browser after initial download

---

## Target Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Proposed Local Deployment                 │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Game Mod (Ed.Bannerboard)                                   │
│  ├─ .NET Framework 4.7.2                                     │
│  ├─ SuperSocket.WebSocket Server                             │
│  ├─ Listens on: ws://localhost:2020                          │
│  └─ Bannerlord Modules Directory                             │
│                                                               │
│                         ↕                                     │
│                   (WebSocket)                                 │
│                         ↕                                     │
│                                                               │
│  Blazor WASM UI (Ed.Bannerboard.UI)                          │
│  ├─ .NET 10.0                                                │
│  ├─ Published to: C:\Bannerboard\wwwroot\                    │
│  ├─ Served via: http://localhost:8080 (or any port)          │
│  └─ Connects to: ws://localhost:2020                         │
│                                                               │
│                         ↑                                     │
│                  (HTTP Static Files)                          │
│                         ↑                                     │
│                                                               │
│  Local Web Server (Choose One Option)                        │
│  ├─ Option A: IIS Express (bundled with Visual Studio)       │
│  ├─ Option B: .NET Kestrel (dotnet serve tool)               │
│  ├─ Option C: Python SimpleHTTPServer                        │
│  ├─ Option D: Node.js 'serve' package                        │
│  └─ Option E: nginx for Windows                              │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

---

## Prerequisites

### Required Software
1. **.NET 10.0 SDK** (or later)
   - Download: https://dotnet.microsoft.com/download/dotnet/10.0
   - Check version: `dotnet --version`
   - Required for building Blazor WebAssembly project

2. **Web Server** (Choose one based on preference):

   **Option A: IIS Express** (Easiest if Visual Studio installed)
   - Already included with Visual Studio
   - No additional installation needed

   **Option B: dotnet serve** (Recommended - simplest)
   - Install: `dotnet tool install -g dotnet-serve`
   - Lightweight .NET global tool
   - Auto-configured for Blazor WASM

   **Option C: Python HTTP Server** (If Python already installed)
   - Python 3.x: `python -m http.server 8080`
   - Requires Python 3.x installation

   **Option D: Node.js 'serve'** (If Node.js already installed)
   - Install: `npm install -g serve`
   - Requires Node.js/npm installation

   **Option E: nginx** (Advanced users)
   - Download Windows binaries from nginx.org
   - Requires manual configuration

### Optional Software
- **Git** (if pulling updates from repository)
- **Visual Studio 2022** or **VS Code** (for making code changes)
- **PowerShell 7+** (for automation scripts)

### System Requirements
- **Operating System**: Windows 10/11 (or any OS running Mount & Blade II)
- **Disk Space**: ~500 MB for published Blazor WASM app
- **Available Ports**: 
  - Port 2020: WebSocket server (already in use by game mod)
  - Port 8080: HTTP server (or any available port of your choice)
- **Browser**: Modern browser with WebSocket support (Chrome, Edge, Firefox)

---

## Code Changes Required

### No Major Code Changes Needed!
The existing codebase is already configured for local connections:

**Current Connection Logic** (`Ed.Bannerboard.UI\Pages\Index.razor.cs:63`):
```csharp
await _webSocket.ConnectAsync(new Uri("ws://localhost:2020"), _disposalTokenSource.Token);
```

**Analysis**: The UI already connects to `localhost:2020`, which means:
- ✅ No hardcoded Azure URLs in WebSocket connection
- ✅ Works from any domain/origin (localhost, file://, or Azure)
- ✅ Blazor WASM downloads once, then runs locally in browser
- ✅ No CORS issues since WebSocket server is localhost

### Optional Configuration Changes
These are marked with `TODO` comments in the codebase and are **optional**:

1. **Make WebSocket Port Configurable** (Optional Enhancement)
   - Current: Hardcoded `2020` in `Init.cs` and `Index.razor.cs`
   - Enhancement: Move to `appsettings.json` configuration
   - Benefit: Easier port changes without recompiling

2. **Add Connection Retry Logic** (Optional Enhancement)
   - Current: Single connection attempt, silent failure
   - Enhancement: Retry failed connections with exponential backoff
   - Benefit: Better UX if game mod starts after UI loads

---

## Implementation Plan

### Phase 1: Build and Publish Blazor WASM App

**Step 1: Open Terminal in Project Directory**
```powershell
cd "c:\Users\Travis\Documents\GitHub\bannerboard\Ed.Bannerboard.UI"
```

**Step 2: Build the Project (Verify Compilation)**
```powershell
dotnet build
```
Expected Output: `Build succeeded`

**Step 3: Publish for Production**
```powershell
dotnet publish -c Release -o "C:\Bannerboard\wwwroot"
```

**What This Does**:
- Compiles Blazor WebAssembly to .NET intermediate language
- Transpiles to WebAssembly (`.wasm` files)
- Minifies/bundles CSS and JavaScript
- Copies all static assets (`wwwroot/*`) to output
- Creates production-optimized build

**Expected Output Directory Structure**:
```
C:\Bannerboard\wwwroot\
├─ _framework/               (WebAssembly runtime and app DLLs)
│  ├─ blazor.webassembly.js
│  ├─ dotnet.*.wasm
│  ├─ *.dll.br (Brotli compressed assemblies)
│  └─ ...
├─ css/                      (Bootswatch Bootstrap theme)
├─ img/                      (Images)
├─ appsettings.json          (Dashboard version config)
├─ favicon.png
└─ index.html                (Entry point)
```

---

### Phase 2: Choose and Configure Web Server

**Recommendation**: Use **dotnet serve** for simplicity and Blazor compatibility.

#### Option A: dotnet serve (Recommended)

**Installation**:
```powershell
dotnet tool install -g dotnet-serve
```

**Run Server**:
```powershell
cd "C:\Bannerboard\wwwroot"
dotnet serve -p 8080 --open-browser
```

**Features**:
- ✅ Automatically serves Blazor WASM with correct MIME types
- ✅ Handles `.br` (Brotli) and `.gz` (Gzip) compression
- ✅ Auto-opens browser to `http://localhost:8080`
- ✅ Supports HTTPS with `--tls` flag

**Create PowerShell Launcher Script**:
```powershell
# Save as: C:\Bannerboard\StartBannerboard.ps1
Set-Location "C:\Bannerboard\wwwroot"
Write-Host "Starting Bannerboard Dashboard..." -ForegroundColor Green
Write-Host "Make sure Mount & Blade II is running with the Bannerboard mod loaded!" -ForegroundColor Yellow
dotnet serve -p 8080 --open-browser
```

**Usage**:
```powershell
.\StartBannerboard.ps1
```

---

#### Option B: IIS Express

**Create IIS Express Config**:
Save as `C:\Bannerboard\applicationhost.config`:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.applicationHost>
    <sites>
      <site name="Bannerboard" id="1">
        <application path="/">
          <virtualDirectory path="/" physicalPath="C:\Bannerboard\wwwroot" />
        </application>
        <bindings>
          <binding protocol="http" bindingInformation="*:8080:localhost" />
        </bindings>
      </site>
    </sites>
  </system.applicationHost>
  <system.webServer>
    <staticContent>
      <mimeMap fileExtension=".wasm" mimeType="application/wasm" />
      <mimeMap fileExtension=".br" mimeType="application/brotli" />
      <mimeMap fileExtension=".dll" mimeType="application/octet-stream" />
    </staticContent>
    <httpCompression>
      <scheme name="br" dll="%ProgramFiles%\IIS Express\iisbrotli.dll" />
    </httpCompression>
  </system.webServer>
</configuration>
```

**Run IIS Express**:
```powershell
& "C:\Program Files\IIS Express\iisexpress.exe" /config:"C:\Bannerboard\applicationhost.config" /site:"Bannerboard"
```

**Create Launcher Script**:
```powershell
# Save as: C:\Bannerboard\StartBannerboard_IIS.ps1
Write-Host "Starting Bannerboard with IIS Express..." -ForegroundColor Green
& "C:\Program Files\IIS Express\iisexpress.exe" /config:"C:\Bannerboard\applicationhost.config" /site:"Bannerboard"
Write-Host "Open browser to: http://localhost:8080" -ForegroundColor Cyan
```

---

#### Option C: Python SimpleHTTPServer

**Run Server** (Python 3.x):
```powershell
cd "C:\Bannerboard\wwwroot"
python -m http.server 8080
```

**Limitations**:
- ⚠️ Does NOT serve `.br` (Brotli) compressed files correctly
- ⚠️ May have incorrect MIME types for `.wasm` files
- ⚠️ Use only if dotnet serve unavailable

---

#### Option D: Node.js 'serve'

**Installation**:
```powershell
npm install -g serve
```

**Run Server**:
```powershell
cd "C:\Bannerboard\wwwroot"
serve -p 8080
```

**Features**:
- ✅ Handles compression and MIME types correctly
- ✅ Fast and lightweight
- ⚠️ Requires Node.js installed

---

### Phase 3: Testing and Validation

**Pre-Flight Checklist**:
1. ✅ Mount & Blade II: Bannerlord is running
2. ✅ Bannerboard mod is loaded in game
3. ✅ Game is in campaign mode (not main menu)
4. ✅ Web server is running on port 8080
5. ✅ Port 2020 is not blocked by firewall

**Test Procedure**:

**Step 1: Verify WebSocket Server**
- Load a campaign in Mount & Blade II
- Check game logs/console for WebSocket server startup
- Expected: Server listening on port 2020

**Step 2: Access Dashboard**
- Open browser to `http://localhost:8080`
- Expected: Blazor loading screen → Dashboard UI

**Step 3: Verify Connection**
- Check browser Developer Tools Console (F12)
- Look for WebSocket connection logs
- Expected: `WebSocket connection established to ws://localhost:2020`

**Step 4: Verify Data Flow**
- Dashboard should show game statistics (kingdom strength, lords, wars, etc.)
- Data updates as game progresses (hourly ticks)

**Troubleshooting**:

| Issue | Diagnosis | Solution |
|-------|-----------|----------|
| Dashboard loads but shows no data | WebSocket connection failed | Ensure game is running with mod loaded |
| "Loading..." stuck on screen | Blazor app failed to initialize | Check browser console for errors, verify `.wasm` files served correctly |
| Connection refused | Port 2020 blocked | Check Windows Firewall, add exception for Mount & Blade II |
| MIME type errors in console | Web server misconfiguration | Use `dotnet serve` or configure MIME types in IIS |

---

### Phase 4: Automation and Convenience

**Create Desktop Shortcut**:
1. Right-click Desktop → New → Shortcut
2. Location: `powershell.exe -File "C:\Bannerboard\StartBannerboard.ps1"`
3. Name: `Bannerboard Dashboard`
4. Change icon to `C:\Bannerboard\wwwroot\favicon.png`

**Create Startup Batch File** (Auto-start with Windows):
Save as `C:\Bannerboard\StartBannerboard.bat`:
```batch
@echo off
cd /d C:\Bannerboard\wwwroot
start http://localhost:8080
dotnet serve -p 8080
```

**Add to Windows Startup** (Optional):
1. Press `Win+R` → `shell:startup`
2. Create shortcut to `StartBannerboard.bat`

---

## Maintenance and Updates

### Updating the Dashboard

**When Bannerboard Releases New Version**:
```powershell
# Pull latest code
cd "c:\Users\Travis\Documents\GitHub\bannerboard"
git pull origin master

# Rebuild and republish
cd Ed.Bannerboard.UI
dotnet publish -c Release -o "C:\Bannerboard\wwwroot"

# Restart web server
```

**Versioning**:
- Dashboard version tracked in `wwwroot\appsettings.json`
- Current version: `0.5.1`
- Update this manually if making local changes

### Backing Up Configuration
Save a copy of:
- `C:\Bannerboard\wwwroot\` (published app)
- `C:\Bannerboard\*.ps1` (launcher scripts)
- Custom configurations (if you modify `appsettings.json`)

---

## Cost Comparison

| Deployment Type | Monthly Cost | Pros | Cons |
|-----------------|--------------|------|------|
| **Azure App Service** (Current) | $13-$55/month | Always available, no local resources | Ongoing cost, internet dependency |
| **Local Hosting** (Proposed) | $0 | No cost, works offline, full control | Must run web server when using dashboard |

**Recommendation**: Local hosting eliminates monthly Azure costs with minimal complexity.

---

## Security Considerations

**Current Security Profile**:
- ✅ WebSocket server only listens on `localhost` (127.0.0.1)
- ✅ No external network exposure
- ✅ No authentication required (single-user local deployment)
- ✅ Blazor WASM runs in browser sandbox

**Warnings**:
- ⚠️ Do NOT expose port 2020 or 8080 to the internet
- ⚠️ Do NOT bind web server to `0.0.0.0` (all interfaces)
- ⚠️ Keep WebSocket connections to `localhost` only

---

## Summary Checklist

**To Deploy Locally**:
- [ ] Install .NET 10.0 SDK
- [ ] Choose web server option (recommend `dotnet serve`)
- [ ] Run `dotnet publish -c Release -o "C:\Bannerboard\wwwroot"`
- [ ] Create PowerShell launcher script
- [ ] Test with game running
- [ ] Create desktop shortcut for convenience

**No Code Changes Required** - Just build, serve, and connect!

---

## Appendix: Advanced Configuration

### Making WebSocket Port Configurable (Optional)

**Step 1: Add to appsettings.json**
```json
{
  "DashboardSettings": {
    "Version": "0.5.1",
    "WebSocketUrl": "ws://localhost:2020"
  }
}
```

**Step 2: Modify Index.razor.cs**
Replace:
```csharp
await _webSocket.ConnectAsync(new Uri("ws://localhost:2020"), _disposalTokenSource.Token);
```

With:
```csharp
var settings = Configuration!.GetSection(nameof(DashboardSettings)).Get<DashboardSettings>();
await _webSocket.ConnectAsync(new Uri(settings!.WebSocketUrl), _disposalTokenSource.Token);
```

**Step 3: Update DashboardSettings Model**
Add property to `Ed.Bannerboard.UI\Models\DashboardSettings.cs`:
```csharp
public string WebSocketUrl { get; set; } = "ws://localhost:2020";
```

**Benefit**: Change WebSocket URL without recompiling.

---

## Support and Resources

- **Bannerboard GitHub**: https://github.com/edgarssults/Bannerboard
- **.NET 10 Documentation**: https://docs.microsoft.com/en-us/dotnet/
- **Blazor WebAssembly Guide**: https://docs.microsoft.com/en-us/aspnet/core/blazor/
- **dotnet serve Tool**: https://github.com/natemcmaster/dotnet-serve

---

**Document Version**: 1.0  
**Date**: December 1, 2025  
**Author**: GitHub Copilot  
**Project**: Bannerboard Local Deployment Migration
