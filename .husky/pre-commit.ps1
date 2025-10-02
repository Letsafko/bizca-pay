#!/usr/bin/env pwsh

# Clean up the solution
jb cleanupcode ".\${@(gci *.sln)[0].Name}"

exit 0