/**
 * Jenkinsfile for generating and validating a CycloneDX SBOM for a .NET project.
 *
 * Prerequisites on the Windows Jenkins agent:
 * 1. .NET SDK (for the 'dotnet' command)
 * 2. CycloneDX .NET Tool (install with: dotnet tool install --global CycloneDX)
 * 3. CycloneDX CLI executable downloaded and placed in a known location.
 */
pipeline {
    agent any

    environment {
        // **UPDATED**: This now points to the downloaded .exe validator tool.
        // Ensure you have downloaded 'cyclonedx-win-x64.exe' and renamed it to 'cyclonedx-cli.exe' in this location.
        CYCLONEDX_CLI_PATH = 'C:\\tools\\cyclonedx-win-x64.exe'

        // Define the name for the generated SBOM file.
        SBOM_NAME = 'sbom.json'

        // **CRITICAL FIX**: Defines the full path to the dotnet-cyclonedx generator tool.
        // Replace this with the actual path from running 'where dotnet-cyclonedx' in your terminal.
        CYCLONEDX_TOOL_PATH = 'C:\\Users\\HP User\\.dotnet\\tools\\dotnet-cyclonedx.exe'
    }

    stages {
        stage('Build Project') {
            steps {
                echo 'Building the .NET project to ensure it is valid...'
                // Explicitly target the project file inside the ConsoleApp directory.
                bat 'dotnet build ConsoleApp/ConsoleApp.csproj'
            }
        }

        stage('Generate CycloneDX SBOM') {
            steps {
                echo "Generating CycloneDX SBOM (${env.SBOM_NAME})..."
                
                // Call the generator tool using its full, absolute path.
                bat "\"${env.CYCLONEDX_TOOL_PATH}\" ConsoleApp/ConsoleApp.csproj -o . --json"

                // **FIX**: Clean up old SBOM before renaming to prevent conflicts.
                bat "if exist ${env.SBOM_NAME} ( del ${env.SBOM_NAME} )"

                // Rename the default output 'bom.json' to our desired name.
                bat "if exist bom.json ( ren bom.json ${env.SBOM_NAME} )"
            }
        }

        stage('Validate SBOM') {
            steps {
                echo "Validating the generated SBOM: ${env.SBOM_NAME}"
                
                // **FIX**: The command now calls the validator executable directly, instead of using 'java -jar'.
                bat "\"${env.CYCLONEDX_CLI_PATH}\" validate --input-file \"${env.SBOM_NAME}\""
            }
        }
    }
    post {
        always {
            // This block runs after all stages, regardless of success or failure.
            echo 'Pipeline finished. Archiving SBOM...'
            // Archive the generated SBOM so it can be downloaded from the Jenkins job UI.
            archiveArtifacts artifacts: "${env.SBOM_NAME}", allowEmptyArchive: true
        }
    }
}
