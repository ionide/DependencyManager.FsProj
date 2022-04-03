$existing = git tag -l
[xml]$buildProps = Get-Content -Path ".\Directory.Build.props"
$version = $buildProps.Project.PropertyGroup.Version

if ($existing -contains $version) {
    Write-Host "Tag $version already exists"
} else {
    git tag v$version
    git push --tags
}