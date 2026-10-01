# ============================================================
#  KrmShn Net Core Backend Project - Visual Studio Template Builder
# ============================================================
#  Usage: ./build-template.ps1
#  Output: ./KrmShnNetCoreBackendProject.zip  (VS template zip)
# ============================================================

$ErrorActionPreference = "Stop"

$RootDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$OutDir    = Join-Path $RootDir "TemplateOutput"
$FinalZip  = Join-Path $RootDir "KrmShnNetCoreBackendProject.zip"

# ---- Clear previous build -------------------------------------
if (Test-Path $OutDir) { Remove-Item -Path $OutDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path (Join-Path $OutDir "Domain"), `
                                          (Join-Path $OutDir "Application"), `
                                          (Join-Path $OutDir "Infrastructure"), `
                                          (Join-Path $OutDir "Api") | Out-Null

# ==============================================================
#  Helper: copy folder tree applying string replacements
# ============================================================
function Copy-TemplateFolder {
    param(
        [string]$Source,
        [string]$Dest,
        [string[]]$ExtraExclude = @()
    )

    $DirExcludes    = @("bin","obj","node_modules","packages",".vs",".git",".trae")
    $ExtExcludes    = @("*.user","*.suo","*.cache","*.log","*.db","*.mdf","*.ldf","*.pdb","*.dll","*.exe","*.deps.json","*.runtimeconfig.json","*.staticwebassets.*")
    $AllExtExcludes = $ExtExcludes + $ExtraExclude

    Get-ChildItem -Path $Source -Recurse -File | ForEach-Object {
        $rel     = $_.FullName.Substring($Source.Length + 1)
        # Normalize to forward-slash for cross-platform path comparison
        $relNorm = $rel -replace "\\", "/"
        $segments= $relNorm -split "/"

        # Directory segment exclude (bin, obj, .trae, etc.)
        foreach ($d in $DirExcludes) { if ($segments -contains $d) { return } }

        # Extension/wildcard exclude
        foreach ($ex in $AllExtExcludes) { if ($relNorm -like $ex) { return } }

        # Path pattern exclude (both slash variants)
        if ($relNorm -like "Migrations/*Designer.cs") { return }
        if ($relNorm -like "Migrations/*ModelSnapshot.cs") { return }

        # Rename files that contain "NetCoreTemplate" -> "$safeprojectname$"
        $newRel = $rel -replace "NetCoreTemplate", "`$safeprojectname`$"

        $destPath = Join-Path $Dest $newRel
        $destDir  = Split-Path $destPath -Parent
        if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Force -Path $destDir | Out-Null }

        # Read as text (most files) — if binary, skip replacement
        $content = [System.IO.File]::ReadAllBytes($_.FullName)
        $isBinary = $false
        try {
            $text = [System.Text.Encoding]::UTF8.GetString($content)
            if ($text -match "[\x00-\x08\x0B\x0C\x0E-\x1F]") { $isBinary = $true }
        } catch { $isBinary = $true }

        if ($isBinary -or $_.Extension -in @(".png",".ico",".jpg",".jpeg",".gif",".pdf",".zip")) {
            Copy-Item $_.FullName -Destination $destPath -Force
        } else {
            $t = $text
            $t = $t -replace "NetCoreTemplate", "`$safeprojectname`$"
            # Replace GUIDs in .slnx / .csproj references with template placeholders
            $t = $t -replace [regex]::Escape("00000000-0000-0000-0000-000000000001"), "`$guid1`$"
            [System.IO.File]::WriteAllText($destPath, $t, [System.Text.Encoding]::UTF8)
        }
    }
}

Write-Host "[1/5] Copying Domain project..."
Copy-TemplateFolder -Source (Join-Path $RootDir "Core\NetCoreTemplate.Domain") `
                    -Dest   (Join-Path $OutDir "Domain")

Write-Host "[2/5] Copying Application project..."
Copy-TemplateFolder -Source (Join-Path $RootDir "Core\NetCoreTemplate.Application") `
                    -Dest   (Join-Path $OutDir "Application")

Write-Host "[3/5] Copying Infrastructure project..."
Copy-TemplateFolder -Source (Join-Path $RootDir "Core\NetCoreTemplate.Infrastructure") `
                    -Dest   (Join-Path $OutDir "Infrastructure")

Write-Host "[4/5] Copying Api project..."
Copy-TemplateFolder -Source (Join-Path $RootDir "Presentation\NetCoreTemplate.Api") `
                    -Dest   (Join-Path $OutDir "Api")

# ==============================================================
#  Generate per-project .vstemplate files
# ==============================================================

# --- Domain.vstemplate ---
Write-Host "Writing Domain.vstemplate"
$projFiles = Get-ChildItem -Path (Join-Path $OutDir "Domain") -Recurse -File |
    Select-Object -ExpandProperty FullName

$domainVst = @"
<VSTemplate Version="3.0.0" Type="Project" xmlns="http://schemas.microsoft.com/developer/vstemplate/2005">
  <TemplateData>
    <Name>`$safeprojectname`$.Domain</Name>
    <Description>Domain Layer — Entities, Enums, Interfaces</Description>
    <ProjectType>CSharp</ProjectType>
    <SortOrder>10</SortOrder>
    <CreateNewFolder>false</CreateNewFolder>
    <DefaultName>Domain</DefaultName>
    <ProvideDefaultName>true</ProvideDefaultName>
  </TemplateData>
  <TemplateContent>
    <Project TargetFileName="`$safeprojectname`$.Domain.csproj" File="`$safeprojectname`$.Domain.csproj" ReplaceParameters="true">
"@
$projFiles | ForEach-Object {
    $rel = $_.Substring((Join-Path $OutDir "Domain").Length + 1)
    if ($rel -notlike "*.vstemplate" -and $rel -notlike "*.csproj") {
        $domainVst += "      <ProjectItem ReplaceParameters=`"true`" TargetFileName=`"$rel`">$rel</ProjectItem>`r`n"
    }
}
$domainVst += @"
    </Project>
  </TemplateContent>
</VSTemplate>
"@
[System.IO.File]::WriteAllText((Join-Path $OutDir "Domain\Domain.vstemplate"), $domainVst, [System.Text.Encoding]::UTF8)

# --- Application.vstemplate ---
Write-Host "Writing Application.vstemplate"
$projFiles = Get-ChildItem -Path (Join-Path $OutDir "Application") -Recurse -File |
    Select-Object -ExpandProperty FullName
$appVst = @"
<VSTemplate Version="3.0.0" Type="Project" xmlns="http://schemas.microsoft.com/developer/vstemplate/2005">
  <TemplateData>
    <Name>`$safeprojectname`$.Application</Name>
    <Description>Application Layer — DTOs, CQRS, Validators, Mappings</Description>
    <ProjectType>CSharp</ProjectType>
    <SortOrder>20</SortOrder>
    <CreateNewFolder>false</CreateNewFolder>
    <DefaultName>Application</DefaultName>
    <ProvideDefaultName>true</ProvideDefaultName>
  </TemplateData>
  <TemplateContent>
    <Project TargetFileName="`$safeprojectname`$.Application.csproj" File="`$safeprojectname`$.Application.csproj" ReplaceParameters="true">
"@
$projFiles | ForEach-Object {
    $rel = $_.Substring((Join-Path $OutDir "Application").Length + 1)
    if ($rel -notlike "*.vstemplate" -and $rel -notlike "*.csproj") {
        $appVst += "      <ProjectItem ReplaceParameters=`"true`" TargetFileName=`"$rel`">$rel</ProjectItem>`r`n"
    }
}
$appVst += @"
    </Project>
  </TemplateContent>
</VSTemplate>
"@
[System.IO.File]::WriteAllText((Join-Path $OutDir "Application\Application.vstemplate"), $appVst, [System.Text.Encoding]::UTF8)

# --- Infrastructure.vstemplate ---
Write-Host "Writing Infrastructure.vstemplate"
$projFiles = Get-ChildItem -Path (Join-Path $OutDir "Infrastructure") -Recurse -File |
    Select-Object -ExpandProperty FullName
$infVst = @"
<VSTemplate Version="3.0.0" Type="Project" xmlns="http://schemas.microsoft.com/developer/vstemplate/2005">
  <TemplateData>
    <Name>`$safeprojectname`$.Infrastructure</Name>
    <Description>Infrastructure Layer — EF Core, UoW, Hangfire, Serilog, Email, Storage</Description>
    <ProjectType>CSharp</ProjectType>
    <SortOrder>30</SortOrder>
    <CreateNewFolder>false</CreateNewFolder>
    <DefaultName>Infrastructure</DefaultName>
    <ProvideDefaultName>true</ProvideDefaultName>
  </TemplateData>
  <TemplateContent>
    <Project TargetFileName="`$safeprojectname`$.Infrastructure.csproj" File="`$safeprojectname`$.Infrastructure.csproj" ReplaceParameters="true">
"@
$projFiles | ForEach-Object {
    $rel = $_.Substring((Join-Path $OutDir "Infrastructure").Length + 1)
    if ($rel -notlike "*.vstemplate" -and $rel -notlike "*.csproj") {
        $infVst += "      <ProjectItem ReplaceParameters=`"true`" TargetFileName=`"$rel`">$rel</ProjectItem>`r`n"
    }
}
$infVst += @"
    </Project>
  </TemplateContent>
</VSTemplate>
"@
[System.IO.File]::WriteAllText((Join-Path $OutDir "Infrastructure\Infrastructure.vstemplate"), $infVst, [System.Text.Encoding]::UTF8)

# --- Api.vstemplate ---
Write-Host "Writing Api.vstemplate"
$projFiles = Get-ChildItem -Path (Join-Path $OutDir "Api") -Recurse -File |
    Select-Object -ExpandProperty FullName
$apiVst = @"
<VSTemplate Version="3.0.0" Type="Project" xmlns="http://schemas.microsoft.com/developer/vstemplate/2005">
  <TemplateData>
    <Name>`$safeprojectname`$.Api</Name>
    <Description>Presentation Layer — Minimal API Endpoints, SignalR Hub, Swagger, Scalar UI</Description>
    <ProjectType>CSharp</ProjectType>
    <SortOrder>40</SortOrder>
    <CreateNewFolder>false</CreateNewFolder>
    <DefaultName>Api</DefaultName>
    <ProvideDefaultName>true</ProvideDefaultName>
  </TemplateData>
  <TemplateContent>
    <Project TargetFileName="`$safeprojectname`$.Api.csproj" File="`$safeprojectname`$.Api.csproj" ReplaceParameters="true">
"@
$projFiles | ForEach-Object {
    $rel = $_.Substring((Join-Path $OutDir "Api").Length + 1)
    if ($rel -notlike "*.vstemplate" -and $rel -notlike "*.csproj") {
        $apiVst += "      <ProjectItem ReplaceParameters=`"true`" TargetFileName=`"$rel`">$rel</ProjectItem>`r`n"
    }
}
$apiVst += @"
    </Project>
  </TemplateContent>
</VSTemplate>
"@
[System.IO.File]::WriteAllText((Join-Path $OutDir "Api\Api.vstemplate"), $apiVst, [System.Text.Encoding]::UTF8)

# ==============================================================
#  Generate solution file (.slnx) with template GUIDs
# ==============================================================
Write-Host "Generating solution (.slnx) file..."

$slnx = @"
<Solution>
  <Folder Name="/Core/">
    <Project Path="Core/`$safeprojectname`$.Domain/`$safeprojectname`$.Domain.csproj" />
    <Project Path="Core/`$safeprojectname`$.Application/`$safeprojectname`$.Application.csproj" />
    <Project Path="Core/`$safeprojectname`$.Infrastructure/`$safeprojectname`$.Infrastructure.csproj" />
  </Folder>
  <Folder Name="/Presentation/">
    <Project Path="Presentation/`$safeprojectname`$.Api/`$safeprojectname`$.Api.csproj" />
  </Folder>
</Solution>
"@
[System.IO.File]::WriteAllText((Join-Path $OutDir "`$safeprojectname`$.slnx"), $slnx, [System.Text.Encoding]::UTF8)

# Create root multi-project .vstemplate
Write-Host "Writing root multi-project .vstemplate"
$rootVst = @"
<VSTemplate Version="3.0.0" xmlns="http://schemas.microsoft.com/developer/vstemplate/2005" Type="ProjectGroup">
  <TemplateData>
    <Name>KrmShn Net Core Backend Project</Name>
    <Description>Production Grade Clean Architecture .NET 11 Backend Template. Identity+RBAC, 2FA, JWT, CQRS (MediatR), Hangfire, SignalR, OData v4, OpenTelemetry, Rate Limiting, Idempotency, Soft-Delete + Audit, File Storage, 43+ Endpoints.</Description>
    <ProjectType>CSharp</ProjectType>
    <SortOrder>1000</SortOrder>
    <CreateNewFolder>true</CreateNewFolder>
    <DefaultName>KrmShnBackend</DefaultName>
    <ProvideDefaultName>true</ProvideDefaultName>
    <LocationField>Enabled</LocationField>
    <EnableLocationBrowseButton>true</EnableLocationBrowseButton>
    <Icon>__TemplateIcon.ico</Icon>
    <PreviewImage>__PreviewImage.png</PreviewImage>
  </TemplateData>
  <TemplateContent>
    <ProjectCollection>
      <ProjectItem ReplaceParameters="true" TargetFileName="`$safeprojectname`$.slnx">`$safeprojectname`$.slnx</ProjectItem>
      <SolutionFolder Name="Core">
        <ProjectTemplateLink ProjectName="`$safeprojectname`$.Domain" CopyParameters="true">Domain\Domain.vstemplate</ProjectTemplateLink>
        <ProjectTemplateLink ProjectName="`$safeprojectname`$.Application" CopyParameters="true">Application\Application.vstemplate</ProjectTemplateLink>
        <ProjectTemplateLink ProjectName="`$safeprojectname`$.Infrastructure" CopyParameters="true">Infrastructure\Infrastructure.vstemplate</ProjectTemplateLink>
      </SolutionFolder>
      <SolutionFolder Name="Presentation">
        <ProjectTemplateLink ProjectName="`$safeprojectname`$.Api" CopyParameters="true">Api\Api.vstemplate</ProjectTemplateLink>
      </SolutionFolder>
    </ProjectCollection>
  </TemplateContent>
</VSTemplate>
"@
[System.IO.File]::WriteAllText((Join-Path $OutDir "KrmShnNetCoreBackendProject.vstemplate"), $rootVst, [System.Text.Encoding]::UTF8)

# ==============================================================
#  Icon + README for the template
# ==============================================================
# Generate a simple icon placeholder (an embedded 1x1 ico is fine for template)
$iconBytes = [byte[]](0x00,0x00,0x01,0x00,0x01,0x00,0x01,0x01,0x00,0x00,0x01,0x00,0x20,0x00,0x68,0x04,
                      0x00,0x00,0x16,0x00,0x00,0x00,0x28,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x02,0x00,
                      0x00,0x00,0x01,0x00,0x20,0x00,0x00,0x00,0x00,0x00,0x00,0x04,0x00,0x00,0x00,0x00,
                      0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                      0x00,0x00,0xFF,0xFF,0x00,0x00)
[System.IO.File]::WriteAllBytes((Join-Path $OutDir "__TemplateIcon.ico"), $iconBytes)
[System.IO.File]::WriteAllBytes((Join-Path $OutDir "__PreviewImage.png"), $iconBytes)

# ==============================================================
#  Pack into .zip (Visual Studio template archive)
# ==============================================================
Write-Host "[5/5] Packaging template into ZIP..."
if (Test-Path $FinalZip) { Remove-Item $FinalZip -Force }
Compress-Archive -Path (Join-Path $OutDir "*") -DestinationPath $FinalZip -CompressionLevel Optimal

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Template build complete!" -ForegroundColor Green
Write-Host "  Output: $FinalZip" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Installation:" -ForegroundColor Yellow
Write-Host "  Option A (for VS 2022): Copy ZIP to:"
Write-Host "    %USERPROFILE%\Documents\Visual Studio 2022\Templates\ProjectTemplates\"
Write-Host "  Option B: In Visual Studio: Project -> Export Template -> Import"
Write-Host "  Option C: Double-click the .zip to auto-import (if VSIX assoc configured)"
