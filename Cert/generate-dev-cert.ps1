# Cert/generate-dev-cert.ps1

# 1. Генерим пароль и путь к .pfx
$password = [guid]::NewGuid()
$certPath = "$PSScriptRoot\localhost-dev.pfx"

Write-Host "Password generated: $password"
Write-Host "CertPath:    $certPath"

# 2. Генерируем dev‑cert
dotnet dev-certs https -ep $certPath -p $password

# 3. Обновляем user‑secrets для локального запуска (VS / dotnet run)
$projectPath = (Convert-Path "$PSScriptRoot/../EventManager.Api/EventManager.Api.csproj")
dotnet user-secrets --project $projectPath set "Kestrel:Certificates:Default:Password" "$password"
dotnet user-secrets --project $projectPath set "Kestrel:Certificates:Default:Path"     "$certPath"

# 4. Пишем .env рядом с docker-compose.yml
#    (docker-compose автоматически читает .env из текущей директории)
$envFile = (Resolve-Path "$PSScriptRoot/../docker-compose.yml").ProviderPath | 
    Split-Path -Parent |
    Join-Path -ChildPath ".env"

# Создаём или перезаписываем .env
@"
# автоматически сгенерировано generate-dev-cert.ps1
ASPNETCORE_Kestrel__Certificates__Default__Password=$password
ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certificates/aspnetapp.pfx
"@ | Out-File -Encoding ASCII -FilePath $envFile -Force

Write-Host ".env updated at $envFile"
