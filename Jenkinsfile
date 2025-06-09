/**
 * Jenkinsfile for generating and validating a CycloneDX SBOM for a .NET project.
 *
 * Prerequisites on the Windows Jenkins agent:
 * 1. .NET SDK (for the 'dotnet' command)
 * 2. CycloneDX .NET Tool (install with: dotnet tool install --global CycloneDX)
 * 3. Java Runtime Environment (for running the validator .jar)
 * 4. CycloneDX CLI .jar file downloaded and placed in a known location.
 */
pipeline {
    agent any

    environment {
        // IMPORTANT: Update this path to the location of your cyclonedx-cli.jar file.
        CYCLONEDX_CLI_PATH = 'C:\\tools\\cyclonedx-cli-2.8.0.jar'

        // Define the name for the generated SBOM file.
        SBOM_NAME = 'sbom.json'

        // **CRITICAL FIX**: Define the full, absolute path to the dotnet-cyclonedx executable.
        // You MUST find this path on your machine by running 'where dotnet-cyclonedx' in a command prompt
        // and replace the example path below with your actual path.
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
                
                // **FIX**: Removed the incorrect '-p' flag. The path is now the main argument.
                bat "\"${env.CYCLONEDX_TOOL_PATH}\" ConsoleApp/ConsoleApp.csproj -o . --json"

                // We rename the default output 'bom.json' to our desired name.
                // The tool by default might name it bom.json, so this step ensures consistency.
                bat "if exist bom.json ( ren bom.json ${env.SBOM_NAME} )"
            }
        }

        stage('Validate SBOM') {
            steps {
                echo "Validating the generated SBOM: ${env.SBOM_NAME}"
                // This step uses the Java-based CycloneDX CLI to validate the SBOM.
                // The pipeline will fail at this stage if the SBOM is not valid.
                bat "java -jar \"${env.CYCLONEDX_CLI_PATH}\" validate --input-file \"${env.SBOM_NAME}\""
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
