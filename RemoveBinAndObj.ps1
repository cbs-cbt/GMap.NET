Get-ChildItem .\ -include bin,obj,AppPackages -Recurse | foreach ($_) { Write-Host $_.FullName; remove-item $_.fullname -Force -Recurse; }
if ($Error)
{
    Pause
}