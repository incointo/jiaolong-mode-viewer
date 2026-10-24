$ErrorActionPreference = 'Stop'
$taskCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$taskSources = @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'src') -Filter '*.cs' | ForEach-Object FullName)
$taskManifest = Join-Path $PSScriptRoot 'src\app.manifest'
$taskOutput = Join-Path $PSScriptRoot '蛟龙模式查看器.exe'
& $taskCompiler /nologo /target:winexe /platform:x64 /optimize+ /codepage:65001 /warnaserror+ /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "/win32manifest:$taskManifest" "/out:$taskOutput" $taskSources
if ($LASTEXITCODE -ne 0) { throw '编译失败' }
