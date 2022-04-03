[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]
    $Version
)

git tag -a $Version -m "Release $Version"
git push --tags