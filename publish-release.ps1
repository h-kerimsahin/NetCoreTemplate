# ============================================================
#  KrmShn Net Core Backend Project - GitHub Release Publisher
#  Usage: ./publish-release.ps1 -Version 1.0.0 [-PreRelease]
# ============================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    [switch]$PreRelease,
    [string]$Branch = "master"
)

$ErrorActionPreference = "Stop"
$RootDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $RootDir

# ---- 1. Read git remote to detect owner/repo ----------------
try {
    $remote = git config --get remote.origin.url
    if ($remote -match "github\.com[:/](?<owner>[^/]+)/(?<repo>[^/.]+)") {
        $Owner = $Matches.owner
        $Repo  = $Matches.repo
        Write-Host "[OK] GitHub repo detected: $Owner/$Repo" -ForegroundColor Green
    } else {
        throw "GitHub remote URL parse failed: $remote"
    }
} catch {
    Write-Host "[HATA] Git remote okunamadi: $_" -ForegroundColor Red
    exit 1
}

$Tag = "v$Version"
$ZIP = Join-Path $RootDir "KrmShnNetCoreBackendProject.zip"

# ---- 2. Build template ZIP first -----------------------------
Write-Host ""
Write-Host "[1/4] Template ZIP build ediliyor..." -ForegroundColor Cyan
& "./build-template.ps1"
if (-not (Test-Path $ZIP)) { throw "Template ZIP olusmadi: $ZIP" }
$sizeKB = [math]::Round((Get-Item $ZIP).Length / 1KB, 1)
Write-Host "[OK] Template hazir ($sizeKB KB): $ZIP" -ForegroundColor Green

# ---- 3. Commit changes + tag + push --------------------------
Write-Host ""
Write-Host "[2/4] Tag: $Tag olusturuluyor..." -ForegroundColor Cyan
git add -A | Out-Null
git diff --cached --quiet
if (-not $?) {
    git commit -m "chore: release template v$Version" --allow-empty | Out-Null
}
git push origin $Branch 2>&1 | Out-Null

# Tag var ise sil (override)
git tag -d $Tag 2>$null | Out-Null
git push origin :refs/tags/$Tag 2>$null | Out-Null
git tag -a $Tag -m "Release v$Version - KrmShn Net Core Template"
git push origin $Tag | Out-Null
Write-Host "[OK] Tag push edildi -> origin/$Tag" -ForegroundColor Green

# ---- 4. Release olustur (gh CLI varsa direkt, yoksa workflow) -
$ghExists = Get-Command "gh" -ErrorAction SilentlyContinue
$ghAuthOk = $false
if ($ghExists) {
    try {
        gh auth status 2>&1 | Out-Null
        $ghAuthOk = $true
    } catch {}
}

$releaseNotes = @"
## KrmShn Net Core Backend Project - v$Version

Production Grade Clean Architecture .NET 11 Backend Template.

### 📦 Kurulum
1. **KrmShnNetCoreBackendProject.zip** dosyasini indir
2. VS 2022 icin klasore kopyala:
   \`\`\`
   %USERPROFILE%\Documents\Visual Studio 2022\Templates\ProjectTemplates\
   \`\`\`
3. Visual Studio'yu yeniden baslat
4. *Create a new project* > arama: **KrmShn** > Template'i sec

### ✨ Ozellikler
- Clean Architecture 4 katman, CQRS (MediatR), Minimal API
- JWT + 2FA + RBAC (Role + Permission), SecurityStamp Middleware
- 3 Katmanli Rate Limit, Idempotency, Path Traversal Korumasi
- Hangfire, SignalR Hub, OData v4, OpenTelemetry + Prometheus
- Soft-Delete + Audit, File Storage (Local/Azure), 43+ Endpoint
- Swagger + Scalar UI, Serilog Structured Logging
"@

if ($ghAuthOk) {
    Write-Host ""
    Write-Host "[3/4] GitHub CLI ile release olusturuluyor..." -ForegroundColor Cyan
    $preFlag = if ($PreRelease) { "--prerelease" } else { "" }
    $noteFile = Join-Path $env:TEMP "krmshn_rel_$Version.md"
    Set-Content -Path $noteFile -Value $releaseNotes -Encoding UTF8

    $cmd = "gh release create $Tag $preFlag --repo $Owner/$Repo " +
           "--title `"KrmShn Template v$Version`" " +
           "--notes-file `"$noteFile`" " +
           "`"$ZIP#KrmShnNetCoreBackendProject.zip`""
    Invoke-Expression $cmd
    Write-Host ""
    Write-Host "[OK] Release Yayinlandi!  ->" -ForegroundColor Green
    Write-Host "   https://github.com/$Owner/$Repo/releases/tag/$Tag" -ForegroundColor Blue
} else {
    Write-Host ""
    Write-Host "[3/4] gh CLI bulunamadi / login olunmamis." -ForegroundColor Yellow
    Write-Host "   Tag '$Tag' push edildi. GitHub Actions workflow otomatik olarak release yaratacak." -ForegroundColor Yellow
    Write-Host "   Workflow durumu: https://github.com/$Owner/$Repo/actions" -ForegroundColor Blue
    Write-Host ""
    Write-Host "   Eger workflow yerine elle release yapmak istiyorsaniz:" -ForegroundColor DarkYellow
    Write-Host "   1) https://github.com/$Owner/$Repo/releases/new" -ForegroundColor Blue
    Write-Host "   2) Tag: $Tag, Title: KrmShn Template v$Version"
    Write-Host "   3) Release notes yapistir, ZIP dosyasini (KrmShnNetCoreBackendProject.zip) upload et"
    Write-Host "   4) Publish release"
}

# ---- Done -----------------------------------------------------
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Bitti. Indirme adresi:" -ForegroundColor Green
Write-Host "  https://github.com/$Owner/$Repo/releases/latest" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Green
