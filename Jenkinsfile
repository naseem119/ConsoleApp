pipeline {
    agent any

    environment {
        // This is still needed to ensure the installed tool is found
        PATH = "${env.PATH};${env.USERPROFILE}\\.dotnet\\tools"
    }

    stages {
        stage('Restore') {
            steps {
                bat 'dotnet restore ConsoleApp/ConsoleApp.csproj'
            }
        }

        stage('Build') {
            steps {
                bat 'dotnet build ConsoleApp/ConsoleApp.csproj --no-restore'
            }
        }

        // *** NEW STAGE TO INSTALL THE TOOL ***
        stage('Install CycloneDX Tool') {
            steps {
                // Installs the CycloneDX .NET tool.
                // Using 'update' is safer as 'install' will fail if it's already installed.
                // Or use 'install' and ignore the exit code if it's already there.
                bat 'dotnet tool install --global CycloneDX.DotNet'
            }
        }

        stage('Generate SBOM') {
            steps {
                bat 'dotnet cyclonedx -p ConsoleApp/ConsoleApp.csproj -o sbom.json'
            }
        }

        stage('Validate SBOM') {
            steps {
                bat '''
                if exist sbom.json (
                    echo SBOM file created successfully.
                ) else (
                    echo ERROR: SBOM file was not created.
                    exit /b 1
                )
                '''
            }
        }
    }

    post {
        always {
            // This will now archive the sbom.json file upon successful generation
            archiveArtifacts artifacts: 'sbom.json', fingerprint: true
        }
    }
}