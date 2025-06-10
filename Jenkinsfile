/**
 * Jenkinsfile for generating, signing, and validating CycloneDX SBOMs for a .NET project.
 *
 * This pipeline generates SBOMs in both XML and JSON formats, signs them using a private key,
 * and verifies the signatures using the corresponding public key.
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
                // The '-fn' flag sets the output filename directly, which is cleaner than renaming.
                bat "\"${env.CYCLONEDX_TOOL_PATH}\" ConsoleApp/ConsoleApp.csproj -o . --json -fn \"${env.SBOM_JSON}\""

                echo "Generating CycloneDX XML SBOM: ${env.SBOM_XML}"
                bat "\"${env.CYCLONEDX_TOOL_PATH}\" ConsoleApp/ConsoleApp.csproj -o . -fn \"${env.SBOM_XML}\""
            }
        }

        stage('Sign SBOMs') {
            steps {
                echo "Signing JSON SBOM with JWS..."
                // The 'sign' command embeds the signature directly into the BOM file.
                bat "\"${env.CYCLONEDX_CLI_PATH}\" sign --key \"${env.PRIVATE_KEY_PATH}\" --input-file \"${env.SBOM_JSON}\" --output-file \"${env.SBOM_JSON}\""

                echo "Signing XML SBOM with XMLDSig..."
                bat "\"${env.CYCLONEDX_CLI_PATH}\" sign --key \"${env.PRIVATE_KEY_PATH}\" --input-file \"${env.SBOM_XML}\" --output-file \"${env.SBOM_XML}\""
            }
        }

        stage('Verify and Validate SBOMs') {
            steps {
                echo "Verifying and validating JSON SBOM..."
                // The 'verify' command checks the signature against the public key.
                bat "\"${env.CYCLONEDX_CLI_PATH}\" verify --key \"${env.PUBLIC_KEY_PATH}\" --input-file \"${env.SBOM_JSON}\""
                // The 'validate' command checks the file against the CycloneDX schema.
                bat "\"${env.CYCLONEDX_CLI_PATH}\" validate --input-file \"${env.SBOM_JSON}\""

                echo "Verifying and validating XML SBOM..."
                bat "\"${env.CYCLONEDX_CLI_PATH}\" verify --key \"${env.PUBLIC_KEY_PATH}\" --input-file \"${env.SBOM_XML}\""
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
