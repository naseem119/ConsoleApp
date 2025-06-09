pipeline {
    agent any

    environment {
        // IMPORTANT: Update this path to the location of your cyclonedx-cli.jar file.
        CYCLONEDX_CLI_PATH = 'C:\\tools\\cyclonedx-cli-2.8.0.jar'

        // Define the name for the generated SBOM file.
        SBOM_NAME = 'sbom.json'
    }

    stages {
        stage('Build Project') {
            steps {
                echo 'Building the .NET project to ensure it is valid...'
                // Use 'bat' for Windows batch commands.
                // This command finds the .sln file in the current directory and builds it.
                bat 'dotnet build'
            }
        }

        stage('Generate CycloneDX SBOM') {
            steps {
                echo "Generating CycloneDX SBOM (${env.SBOM_NAME})..."
                // **CORRECTED COMMAND**: The official tool command is 'dotnet-cyclonedx'.
                // This command scans the project and generates the SBOM.
                // '.'          - Scan the current directory for a .sln or .csproj file.
                // '-o .'       - Output the SBOM file to the current directory.
                // '--json'     - Specifies the output format should be JSON.
                bat "dotnet-cyclonedx . -o . --json"

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
