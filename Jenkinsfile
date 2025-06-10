/**
 * Jenkinsfile to generate, sign, and validate CycloneDX SBOMs.
 * This pipeline produces both JSON and XML SBOMs, but only signs and verifies the XML version
 * due to a limitation in the current cyclonedx-cli tool.
 */
pipeline {
    agent any

    environment {
        // --- Tool Paths ---
        CYCLONEDX_CLI_PATH = 'C:\\tools\\cyclonedx-win-x64.exe'
        CYCLONEDX_TOOL_PATH = 'C:\\Users\\HP User\\.dotnet\\tools\\dotnet-cyclonedx.exe'

        // --- Signing Key Paths ---
        // These keys should be stored securely on the Jenkins agent machine.
        PRIVATE_KEY_PATH = 'C:\\tools\\keys\\private.key'
        PUBLIC_KEY_PATH = 'C:\\tools\\keys\\public.key'

        // --- Output Filenames ---
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
                echo "Generating JSON SBOM..."
                bat "\"${env.CYCLONEDX_TOOL_PATH}\" ConsoleApp/ConsoleApp.csproj -o . --json -fn \"${env.SBOM_JSON}\""

                echo "Generating XML SBOM..."
                bat "\"${env.CYCLONEDX_TOOL_PATH}\" ConsoleApp/ConsoleApp.csproj -o . -fn \"${env.SBOM_XML}\""
            }
        }

        stage('Sign XML SBOM') {
            steps {
                echo "Signing XML SBOM with XMLDSig..."
                // Signs the XML SBOM in-place using the private key.
                bat "\"${env.CYCLONEDX_CLI_PATH}\" sign bom \"${env.SBOM_XML}\" --key-file \"${env.PRIVATE_KEY_PATH}\""
            }
        }

        stage('Verify and Validate SBOMs') {
            steps {
                echo "Validating JSON SBOM (unsigned)..."
                bat "\"${env.CYCLONEDX_CLI_PATH}\" validate --input-file \"${env.SBOM_JSON}\""

                echo "Verifying and validating XML SBOM (signed)..."
                // Verifies the signature using the public key.
                bat "\"${env.CYCLONEDX_CLI_PATH}\" verify all \"${env.SBOM_XML}\" --key-file \"${env.PUBLIC_KEY_PATH}\""
                // Validates the file format against the CycloneDX schema.
                bat "\"${env.CYCLONEDX_CLI_PATH}\" validate --input-file \"${env.SBOM_XML}\""
            }
        }
    }
    post {
        always {
            echo 'Pipeline finished. Archiving SBOMs...'
            archiveArtifacts artifacts: "${env.SBOM_JSON}, ${env.SBOM_XML}", allowEmptyArchive: true
        }
    }
}
