#V5RPC Protocol Buffer build script
#Created by AzureFx on 2019/4/13
param (
    [Parameter(Position = 0)]
    [string]$SourceDir = $PWD,
    [Parameter(Position = 1)]
    [string]$OutputDir = (Join-Path $SourceDir "Generated"),
    [switch]$Lazy
)

$sourceDir = Resolve-Path $sourceDir -ErrorAction Stop
if (!(Test-Path $outputDir)) {
    New-Item -Path $outputDir -ItemType Directory | Out-Null
}
$outputDir = Resolve-Path $outputDir -ErrorAction Stop

function Get-ProtoSource {
    Get-ChildItem -Path $SourceDir -Filter "*.proto"
}

function Get-ProtoCompiler {
    $compiler = Get-Command 'protoc.exe' -ErrorAction Ignore
    if ($null -eq $compiler) {
        if($IsLinux -Or $IsMacOs) {
            $compiler = Get-Command '.\protoc\bin\protoc' -ErrorAction Ignore
            if($null -ne $compiler) {
                chmod +x $compiler.Source
            }
        } elseif($IsWindows) {
            $compiler = Get-Command 'protoc\bin\protoc.exe' -ErrorAction Ignore
        }
    }
    return $compiler
}

$lastBuildStateFile = Join-Path $OutputDir "LastBuildState.txt"
$latestSourceDate = (Get-ProtoSource | Sort-Object LastWriteTime | Select-Object -Last 1).LastWriteTime

if ($Lazy) {
    $lastBuildState = Get-Content -Path $lastBuildStateFile -ReadCount 1 -ErrorAction Ignore
    try {
        $lastBuildDate = [System.DateTime]::Parse($lastBuildState)
        if ($lastBuildDate.ToString() -eq $latestSourceDate.ToString()) {
            Write-Output "Protocol Buffer 源文件是最新的"
            exit 0
        }
    }
    catch {
        Write-Output "无法获取上一次生成时间"
    }
}

Write-Output "正在生成 Protocol Buffer 文件"

$compiler = Get-ProtoCompiler
if ($null -eq $compiler) {
    try {
        Write-Output '正在下载编译器'
        $url = 'https://github.com/protocolbuffers/protobuf/releases/download/v3.7.1/protoc-3.7.1-win64.zip'
        Invoke-WebRequest $url -OutFile 'protoc.zip' -ErrorAction Stop
        Write-Output '正在提取文件'
        Expand-Archive 'protoc.zip' -ErrorAction Stop
        Remove-Item 'protoc.zip'
    }
    catch {
        Write-Output "无法从 $url 获得编译器"
    }
}
$compiler = Get-ProtoCompiler
if ($null -eq $compiler) {
    Write-Output "error: 无法找到 Protocol Buffer 编译器"
    exit 1
}

function Write-MSBuildError {
    param (
        [Parameter(ValueFromPipeline)]
        [PsCustomObject]$Lines
    )
    process {
        if ($Lines -match '(.+?)\s*:(.*)') {
            $source = $Matches[1]
            $rest = $Matches[2]
            if ($rest -imatch 'warning\s*:(.*)') {
                $rest = $Matches[1]
                "${source}: warning: $rest"
            }
            elseif ($rest -match '(\d+):(\d+):\s*(.+)') {
                $lineno = $Matches[1]
                $colno = $Matches[2]
                $message = $Matches[3]
                "${source}($lineno,$colno): error: $message"
            }
            else {
                if ($Lines -ilike '*warning*') {
                    "${source}: warning: $rest"
                }
                else {
                    "${source}: error: $rest"
                }
            }
        }
        else {
            $Lines
        }
    }
}

function Compile-Proto {
    param (
        [Parameter(Position = 0)]
        [string]$Path
    )
    $Path = Resolve-Path $Path
    &$compiler --proto_path=$sourceDir --csharp_out=$outputDir $Path 2>&1 | ForEach-Object {
        if ($_ -is [System.Management.Automation.ErrorRecord]) {
            Write-MSBuildError $_
        }
        else {
            $_
        }
    }
    if ($LASTEXITCODE -ne 0) {
        Write-Output "error: 编译 $($Path) 时发生了错误"
        throw
    }
    Write-Output "$($Path) 编译成功"
}

Write-Output "已找到编译器 $($compiler.Source)"
Write-Output "源文件目录是 $sourceDir"
Write-Output "输出目录是 $outputDir"

Get-ChildItem -Path $outputDir | Where-Object { $_.Extension -eq ".cs" } | Remove-Item

try {
    foreach ($file in Get-ProtoSource) {
        Compile-Proto -Path $file.FullName
    }
}
catch {
    Write-Error $_
    exit 1
}

Set-Content -Path $lastBuildStateFile -Value $latestSourceDate