#V5RPC Protocol Buffer build script
#Created by AzureFx on 2019/4/13
Write-Output "正在生成 Protocol Buffer 文件"

$sourceDir = Resolve-Path $PWD
$outputDir = Join-Path $PWD "Generated"

if (!(Test-Path $outputDir)) {
    New-Item -Path $outputDir -ItemType Directory | Out-Null
}
$outputDir = Resolve-Path $outputDir

$compiler = Get-Command "protoc.exe" -ErrorAction Ignore
if ($null -eq $compiler) {
    Write-Error "Error: 无法找到 Protocol Buffer 编译器"
    exit 1
}

function Compile-Proto {
    param (
        [Parameter(Position = 0)]
        [string]$Path
    )
    $Path = Resolve-Path $Path
    &$compiler --proto_path=$sourceDir --csharp_out=$outputDir $Path
    if ($LASTEXITCODE -ne 0) {
        throw "Error: 编译 $($Path) 时发生了错误"
    }
}

Write-Output "已找到编译器 $($compiler.Source)"
Write-Output "源文件目录是 $sourceDir"
Write-Output "输出目录是 $outputDir"

Get-ChildItem -Path $outputDir | Where-Object { $_.Extension -eq ".cs" } | Remove-Item

try {
    Compile-Proto -Path ".\API.proto"
    Compile-Proto -Path ".\DataStructures.proto"
    Compile-Proto -Path ".\Events.proto"
}
catch {
    Write-Error $_
    exit 1
}

Write-Output "生成成功"