# ============================================================
# Сборка APK «Табло — пульт» (автор InJeector) БЕЗ Gradle и без сети.
# Используются только инструменты Android SDK (aapt2, d8, zipalign, apksigner)
# и JDK (javac, jar, keytool). Подходит для Windows 7/10/11 + PowerShell 5.1.
#
# Запуск (из папки android):
#   powershell -ExecutionPolicy Bypass -File .\build-apk.ps1
#
# Результат: TabloPult-InJeector-1.2.apk
# ============================================================
[CmdletBinding()]
param(
    [string]$Sdk,
    [string]$Jdk,
    [string]$OutApk
)

$ErrorActionPreference = 'Stop'
$here = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
$appDir = Join-Path $here 'app'

# --- Android SDK ---
if (-not $Sdk) {
    if ($env:ANDROID_HOME) { $Sdk = $env:ANDROID_HOME }
    elseif ($env:ANDROID_SDK_ROOT) { $Sdk = $env:ANDROID_SDK_ROOT }
    else { $Sdk = Join-Path $env:LOCALAPPDATA 'Android\Sdk' }
}
if (-not (Test-Path -LiteralPath $Sdk)) {
    throw "Android SDK не найден: $Sdk. Укажите -Sdk <путь> или задайте ANDROID_HOME."
}

# --- build-tools: самая новая установленная версия ---
$btRoot = Join-Path $Sdk 'build-tools'
$bt = $null
if (Test-Path -LiteralPath $btRoot) {
    $candidates = Get-ChildItem -LiteralPath $btRoot -Directory |
        Where-Object { Test-Path (Join-Path $_.FullName 'aapt2.exe') } |
        Sort-Object { try { [version]$_.Name } catch { [version]'0.0.0' } } -Descending
    if ($candidates -and $candidates.Count -gt 0) { $bt = $candidates[0].FullName }
}
if (-not $bt) {
    throw "Не найдены build-tools (aapt2.exe) в $btRoot."
}

# --- platform: android-34, иначе самый новый android-XX[.0] ---
$platRoot = Join-Path $Sdk 'platforms'
$androidJar = $null
if (Test-Path -LiteralPath $platRoot) {
    $plats = Get-ChildItem -LiteralPath $platRoot -Directory |
        Where-Object { Test-Path (Join-Path $_.FullName 'android.jar') }
    $p34 = $plats | Where-Object { $_.Name -eq 'android-34' } | Select-Object -First 1
    if ($p34) {
        $androidJar = Join-Path $p34.FullName 'android.jar'
    } else {
        $best = $plats | Sort-Object { try { [int](($_.Name -replace '[^0-9].*$','')) } catch { 0 } } -Descending |
            Select-Object -First 1
        if ($best) { $androidJar = Join-Path $best.FullName 'android.jar' }
    }
}
if (-not $androidJar) {
    throw "Не найдена платформа Android (platforms\android-*\android.jar) в $Sdk."
}
Write-Host "SDK:        $Sdk"
Write-Host "build-tools:$bt"
Write-Host "android.jar:$androidJar"

# --- JDK ---
function Find-Jdk {
    $c = New-Object System.Collections.Generic.List[string]
    if ($env:JAVA_HOME) { $c.Add($env:JAVA_HOME) }
    $c.Add('C:\Program Files\Android\Android Studio\jbr')
    $c.Add('C:\Program Files\Java\jdk-17')
    $c.Add('C:\Program Files\Java\jdk-21')
    $c.Add('C:\Program Files\Eclipse Adoptium\jdk-17')
    foreach ($p in $c) {
        if ($p -and (Test-Path (Join-Path $p 'bin\javac.exe'))) { return $p }
    }
    $cmd = Get-Command javac.exe -ErrorAction SilentlyContinue
    if ($cmd) { return (Split-Path -Parent (Split-Path -Parent $cmd.Source)) }
    return $null
}
if (-not $Jdk) { $Jdk = Find-Jdk }
if (-not $Jdk) {
    throw "JDK не найден. Укажите -Jdk <путь> или задайте JAVA_HOME (нужен JDK 17+)."
}
Write-Host "JDK:        $Jdk"

$env:JAVA_HOME = $Jdk
$env:PATH = (Join-Path $Jdk 'bin') + ';' + $env:PATH

$javac = Join-Path $Jdk 'bin\javac.exe'
$jar = Join-Path $Jdk 'bin\jar.exe'
$keytool = Join-Path $Jdk 'bin\keytool.exe'
$aapt2 = Join-Path $bt 'aapt2.exe'
$aapt = Join-Path $bt 'aapt.exe'
$d8 = Join-Path $bt 'd8.bat'
$zipalign = Join-Path $bt 'zipalign.exe'
$apksigner = Join-Path $bt 'apksigner.bat'
foreach ($t in @($javac, $jar, $keytool, $aapt2, $d8, $zipalign, $apksigner)) {
    if (-not (Test-Path -LiteralPath $t)) { throw "Не найден инструмент: $t" }
}

$build = Join-Path $appDir 'build\manual'
if (Test-Path -LiteralPath $build) { Remove-Item -LiteralPath $build -Recurse -Force }
New-Item -ItemType Directory -Force -Path $build | Out-Null
$genDir = Join-Path $build 'gen'
$classesDir = Join-Path $build 'classes'
$dexDir = Join-Path $build 'dex'
New-Item -ItemType Directory -Force -Path $genDir, $classesDir, $dexDir | Out-Null

$baseApk = Join-Path $build 'base-unsigned.apk'
$alignedApk = Join-Path $build 'aligned-unsigned.apk'
$classesJar = Join-Path $build 'classes.jar'
$compiledRes = Join-Path $build 'resources.zip'
$dexFile = Join-Path $dexDir 'classes.dex'
if (-not $OutApk) { $OutApk = Join-Path $here 'TabloPult-InJeector-1.2.apk' }

function Invoke-Tool([string]$exe, [string[]]$arguments, [string]$title) {
    Write-Host "== $title"
    & $exe @arguments
    if ($LASTEXITCODE -ne 0) { throw "${title}: код возврата $LASTEXITCODE" }
}

# --- 1. Манифест с package (AGP подставляет namespace, aapt2 — нет) ---
$manifestSrc = Join-Path $appDir 'src\main\AndroidManifest.xml'
$manifest = Join-Path $build 'AndroidManifest.xml'
$xml = [System.IO.File]::ReadAllText($manifestSrc, [System.Text.Encoding]::UTF8)
if ($xml -notmatch '<manifest\s+package=') {
    $xml = $xml -replace '<manifest\s', '<manifest package="ru.tablo.operator" '
}
[System.IO.File]::WriteAllText($manifest, $xml, (New-Object System.Text.UTF8Encoding($false)))

# --- 2. Ресурсы ---
Invoke-Tool $aapt2 @('compile','--dir',(Join-Path $appDir 'src\main\res'),'-o',$compiledRes) 'aapt2 compile'
Invoke-Tool $aapt2 @('link','-o',$baseApk,'-I',$androidJar,'--manifest',$manifest,'--java',$genDir,
    '--min-sdk-version','26','--target-sdk-version','34','--version-code','4','--version-name','1.2',$compiledRes) 'aapt2 link'

# --- 3. Java -> .class ---
$javaFiles = @()
$javaFiles += Get-ChildItem -LiteralPath (Join-Path $appDir 'src\main\java') -Recurse -Filter *.java | ForEach-Object { $_.FullName }
$javaFiles += Get-ChildItem -LiteralPath $genDir -Recurse -Filter *.java | ForEach-Object { $_.FullName }
if ($javaFiles.Count -eq 0) { throw 'Не найдены .java-исходники.' }
$javacArgs = @('--release','8','-encoding','UTF-8','-classpath',$androidJar,'-d',$classesDir) + $javaFiles
Invoke-Tool $javac $javacArgs 'javac'

# --- 4. .class -> classes.dex ---
Invoke-Tool $jar @('cf',$classesJar,'-C',$classesDir,'.') 'jar'
Invoke-Tool $d8 @('--min-api','26','--lib',$androidJar,'--output',$dexDir,$classesJar) 'd8'

# --- 5. classes.dex внутрь APK ---
Add-Type -AssemblyName System.IO.Compression -ErrorAction SilentlyContinue
Add-Type -AssemblyName System.IO.Compression.FileSystem -ErrorAction SilentlyContinue
$zip = [System.IO.Compression.ZipFile]::Open($baseApk, [System.IO.Compression.ZipArchiveMode]::Update)
try {
    $old = $zip.GetEntry('classes.dex')
    if ($old) { $old.Delete() }
    [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
        $zip, $dexFile, 'classes.dex', [System.IO.Compression.CompressionLevel]::Optimal)
} finally {
    $zip.Dispose()
}

# --- 6. Выравнивание и подпись ---
Invoke-Tool $zipalign @('-f','-p','4',$baseApk,$alignedApk) 'zipalign'

$ksDir = Join-Path $here 'keystore'
$ks = Join-Path $ksDir 'tablo-injeector.keystore'
if (-not (Test-Path -LiteralPath $ks)) {
    New-Item -ItemType Directory -Force -Path $ksDir | Out-Null
    Write-Host '== keytool: создание ключа подписи'
    & $keytool -genkeypair -v -keystore $ks -storetype PKCS12 -storepass android -keypass android `
        -alias androiddebugkey -keyalg RSA -keysize 2048 -validity 10000 -noprompt `
        -dname 'CN=InJeector, OU=Tablo, O=InJeector, L=Yekaterinburg, ST=Sverdlovsk, C=RU'
    if ($LASTEXITCODE -ne 0) { throw 'keytool завершился с ошибкой.' }
}
Invoke-Tool $apksigner @('sign','--ks',$ks,'--ks-key-alias','androiddebugkey',
    '--ks-pass','pass:android','--key-pass','pass:android','--out',$OutApk,$alignedApk) 'apksigner sign'

Write-Host '== apksigner verify'
& $apksigner verify --verbose --print-certs $OutApk
if ($LASTEXITCODE -ne 0) { throw 'Проверка подписи не прошла.' }

Write-Host ''
Write-Host "ГОТОВО: $OutApk"
Write-Host "Размер: $([math]::Round((Get-Item -LiteralPath $OutApk).Length / 1KB, 1)) КБ"
if (Test-Path -LiteralPath $aapt) {
    & $aapt dump badging $OutApk | Select-String -Pattern 'package:|sdkVersion|targetSdkVersion|application-label' | ForEach-Object { $_.Line }
}
exit 0
