# Load environment variables from .env and env.io files
function Import-EnvFile {
    param (
        [string]$EnvFile
    )
    if (Test-Path $EnvFile) {
        $envVars = @()
        Get-Content $EnvFile | ForEach-Object {
            if ($_ -match '^([^#][^=]+)=(.+)$') {
                $key = $matches[1].Trim()
                $value = $matches[2].Trim()
                [System.Environment]::SetEnvironmentVariable($key, $value)
                # Create bash export statement
                $envVars += "${key}=`"${value}`""
            }
        }
        return $envVars -join " && export "
    }
    return ""
}

# Import environment variables and get export strings
$envString1 = Import-EnvFile ".env"
$envString2 = Import-EnvFile ".env.io"

# Combine environment variables for WSL
$wslEnvString = "export $envString1 && export $envString2 &&"

# Create network if it doesn't exist
$networkName = "${env:IO_PROJECT}_${env:IO_APP}_${env:IO_STAGE}"
$networks = wsl bash -c "$wslEnvString docker network ls --format '{{.Name}}'"
if ($networks -notcontains $networkName) {
    Write-Host "Creating network: $networkName"
    wsl bash -c "$wslEnvString docker network create $networkName"
}
# Create devopsnet network if it doesn't exist
$devopsNetName = "devopsnet"
if ($networks -notcontains $devopsNetName) {
    Write-Host "Creating network: $devopsNetName"
    wsl bash -c "$wslEnvString docker network create $devopsNetName"
}

# Create volumes if they don't exist
$volumes = wsl bash -c "$wslEnvString docker volume ls --format '{{.Name}}'"
if ($volumes -notcontains $env:DB_VOLUME_NAME) {
    Write-Host "Creating volume: $env:DB_VOLUME_NAME"
    wsl bash -c "$wslEnvString docker volume create ${env:DB_VOLUME_NAME}"
}
if ($volumes -notcontains $env:PGADMIN_VOLUME_NAME) {
    Write-Host "Creating volume: $env:PGADMIN_VOLUME_NAME"
    wsl bash -c "$wslEnvString docker volume create ${env:PGADMIN_VOLUME_NAME}"
}

# Start the containers using WSL with environment variables
wsl bash -c "$wslEnvString docker compose --profile development up -d postgres pgadmin"

Write-Host "PostgreSQL and pgAdmin are starting up..."
Write-Host "pgAdmin will be available at: http://localhost:$($env:PGADMIN_PORT)"
Write-Host "PostgreSQL is available at: localhost:$($env:DB_PORT)"