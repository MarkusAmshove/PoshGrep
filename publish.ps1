$scriptPath = Split-Path $Script:MyInvocation.MyCommand.Path -Parent
$releaseDir = [System.IO.Path]::Combine($scriptPath, "Release", "PoshGrep")

$releaseFiles = @("PoshGrep.dll", "PoshGrep.psd1")

if(!(Test-Path $releaseDir))
{
    mkdir $releaseDir -Force
}
else
{
    foreach($releaseFile in $releaseFiles) {
        Remove-Item -Path "$releaseDir\$releaseFile"
    }
}

Move-Item -Path "$scriptPath\PoshGrep\bin\Release\PoshGrep.dll" -Destination "$releaseDir"
Move-Item -Path "$scriptPath\PoshGrep\bin\Release\PoshGrep.psd1" -Destination "$releaseDir"

$apiKey = Read-Host -Prompt "PSGallery API-Key:"
Publish-Module -Path $releaseDir -NuGetApiKey $apiKey