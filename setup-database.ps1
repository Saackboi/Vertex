# Script para crear y aplicar migraciones de Entity Framework Core

Write-Host "================================" -ForegroundColor Cyan
Write-Host "VERTEX - Configuración de Base de Datos" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Navegar a la carpeta de la API
$apiPath = Join-Path $PSScriptRoot "src\Vertex.API"
Set-Location $apiPath

Write-Host "📂 Ubicación actual: $apiPath" -ForegroundColor Yellow
Write-Host ""

# Verificar si dotnet-ef está instalado
Write-Host "🔍 Verificando herramientas de EF Core..." -ForegroundColor Yellow
$efVersion = dotnet ef --version 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ dotnet-ef no está instalado. Instalando..." -ForegroundColor Red
    dotnet tool install --global dotnet-ef
    Write-Host "✅ dotnet-ef instalado correctamente" -ForegroundColor Green
} else {
    Write-Host "✅ dotnet-ef ya está instalado: $efVersion" -ForegroundColor Green
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Paso 1: Crear Migración Inicial" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Crear la migración inicial
Write-Host "🏗️  Creando migración 'InitialCreate'..." -ForegroundColor Yellow
dotnet ef migrations add InitialCreate --project ..\Vertex.Infrastructure --startup-project .

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Migración creada exitosamente" -ForegroundColor Green
} else {
    Write-Host "❌ Error al crear la migración" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Paso 2: Aplicar Migración a la BD" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "🗄️  Aplicando migración a la base de datos..." -ForegroundColor Yellow
dotnet ef database update

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Base de datos creada y actualizada correctamente" -ForegroundColor Green
} else {
    Write-Host "❌ Error al aplicar la migración" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "🎉 ¡Configuración Completada!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "La base de datos 'VertexDB' ha sido creada con las siguientes tablas:" -ForegroundColor White
Write-Host "  • OnboardingProcesses" -ForegroundColor Gray
Write-Host "  • ProfessionalProfiles" -ForegroundColor Gray
Write-Host "  • AspNetUsers (y tablas de Identity)" -ForegroundColor Gray
Write-Host ""
Write-Host "Puedes ejecutar la aplicación con:" -ForegroundColor Yellow
Write-Host "  dotnet run --project src/Vertex.API/Vertex.API.csproj" -ForegroundColor Cyan
Write-Host ""
