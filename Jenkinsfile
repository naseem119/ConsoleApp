/**
 * Jenkinsfile for generating, signing, and validating CycloneDX SBOMs for a .NET project.
 *
 * This pipeline generates SBOMs in both XML and JSON formats. It signs and verifies the XML
 * file, as the current version of the cyclonedx-cli tool only supports signing XML files.
 */
pipeline {
    agent any

    environment {
        // --- Paths to validation and generator tools ---
        CYCLONEDX_CLI_PATH = 'C:\\tools\\cyclonedx-win-x64.exe'
        CYCLONEDX_TOOL_PATH = 'C:\\Users\\HP User\\.dotnet\\tools\\dotnet-cyclonedx.exe'

        // --- Paths to your signing keys ---
        // IMPORTANT: Update these paths to where you stored your generated keys.
        PRIVATE_KEY_PATH = 'C:\\tools\\keys\\private.key'
        PUBLIC_KEY_PATH = 'C:\\tools\\keys\\public.key'

        // --- SBOM Filenames ---
        SBOM_JSON = 'sbom.json'
        SBOM_XML = 'sbom.xml'
    }

    stages {
        stage('Build Project') {
            steps {
                echo 'Building the .NET project...'
                bat 'dotnet build ConsoleApp/ConsoleApp.csproj'
            }
        }

        stage('Generate SBOMs (JSON and XML)') {
            steps {
                echo "Generating CycloneDX JSON SBOM: ${env.SBOM_JSON}"
                bat "\"${env.CYCLONEDX_TOOL_PATH}\" ConsoleApp/ConsoleApp.csproj -o . --json -fn \"${env.SBOM_JSON}\""

                echo "Generating CycloneDX XML SBOM: ${env.SBOM_XML}"
                bat "\"${env.CYCLONEDX_TOOL_PATH}\" ConsoleApp/ConsoleApp.csproj -o . -fn \"${env.SBOM_XML}\""
            }
        }

        stage('Sign XML SBOM') {
            steps {
                echo "Signing XML SBOM with XMLDSig..."
                // NOTE: The CLI tool currently only supports signing XML files.
                bat "\"${env.CYCLONEDX_CLI_PATH}\" sign bom \"${env.SBOM_XML}\" --key-file \"${env.PRIVATE_KEY_PATH}\""
            }
        }

        stage('Verify and Validate SBOMs') {
            steps {
                echo "Validating JSON SBOM (unsigned)..."
                bat "\"${env.CYCLONEDX_CLI_PATH}\" validate --input-file \"${env.SBOM_JSON}\""

                echo "Verifying and validating XML SBOM (signed)..."
                // **FIX**: The 'verify' command uses the 'all' subcommand, not 'bom'.
                bat "\"${env.CYCLONEDX_CLI_PATH}\" verify all \"${env.SBOM_XML}\" --key-file \"${env.PUBLIC_KEY_PATH}\""
                bat "\"${env.CYCLONEDX_CLI_PATH}\" validate --input-file \"${env.SBOM_XML}\""
            }
        }
    }
    post {
        always {
            echo 'Pipeline finished. Archiving SBOMs...'
            // Archive both generated SBOMs so they can be downloaded from the Jenkins job UI.
            archiveArtifacts artifacts: "${env.SBOM_JSON}, ${env.SBOM_XML}", allowEmptyArchive: true
        }
    }
}
