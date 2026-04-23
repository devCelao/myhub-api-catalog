# ============================================================================
# run-coverage.ps1
# Executa os testes com coleta de code coverage e gera relatorio HTML.
# Uso: .\run-coverage.ps1
# ============================================================================

$ErrorActionPreference = "Stop"

$SolutionRoot  = $PSScriptRoot
$TestProject   = Join-Path $SolutionRoot "CatalogTest\CatalogTest.csproj"
$RunSettings   = Join-Path $SolutionRoot "coverlet.runsettings"
$ResultsDir    = Join-Path $SolutionRoot "TestResults"
$ReportDir     = Join-Path $SolutionRoot "CoverageReport"

# --- Limpar resultados anteriores ----------------------------------------
if (Test-Path $ResultsDir) { Remove-Item $ResultsDir -Recurse -Force }
if (Test-Path $ReportDir)  { Remove-Item $ReportDir  -Recurse -Force }

# --- Executar testes com coverage -----------------------------------------
Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  Executando testes com coverage..."   -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

dotnet test $TestProject `
    --collect:"XPlat Code Coverage" `
    --settings $RunSettings `
    --results-directory $ResultsDir `
    --verbosity normal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Testes falharam. Abortando." -ForegroundColor Red
    exit $LASTEXITCODE
}

# --- Verificar se o ReportGenerator esta instalado ------------------------
$rgInstalled = dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool"
if (-not $rgInstalled) {
    Write-Host ""
    Write-Host "Instalando ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# --- Localizar o arquivo de coverage gerado ------------------------------
$CoverageFile = Get-ChildItem -Path $ResultsDir -Filter "coverage.cobertura.xml" -Recurse | Select-Object -First 1

if (-not $CoverageFile) {
    Write-Host "Arquivo de coverage nao encontrado em $ResultsDir" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Coverage XML: $($CoverageFile.FullName)" -ForegroundColor Green

# --- Gerar relatorio HTML -------------------------------------------------
Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  Gerando relatorio HTML..."           -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

reportgenerator `
    "-reports:$($CoverageFile.FullName)" `
    "-targetdir:$ReportDir" `
    "-reporttypes:Html;TextSummary"

# --- Exibir resumo no terminal --------------------------------------------
$SummaryFile = Join-Path $ReportDir "Summary.txt"
if (Test-Path $SummaryFile) {
    Write-Host ""
    Write-Host "======================================" -ForegroundColor Green
    Write-Host "  RESUMO DE COVERAGE"                   -ForegroundColor Green
    Write-Host "======================================" -ForegroundColor Green
    Get-Content $SummaryFile | Write-Host
}

# --- Abrir relatorio no browser -------------------------------------------
$IndexFile = Join-Path $ReportDir "index.html"
if (Test-Path $IndexFile) {
    Write-Host ""
    Write-Host "Abrindo relatorio em: $IndexFile" -ForegroundColor Cyan
    Start-Process $IndexFile
}
