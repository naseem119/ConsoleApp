pipeline {
    agent any

    environment {
        // This line is correct and necessary. It ensures the path where
        // the tool gets installed is searchable.
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

        // *** ADD THIS NEW STAGE ***
        stage('Install CycloneDX Tool') {
            steps {
                // This command installs the necessary tool. It will fail if the tool
                // is already installed, which is okay for a clean workspace.
                // For more robust pipelines, one might use a more complex script
                // to check for existence first, but this is the essential command.
                bat 'dotnet tool install --global CycloneDX.DotNet'
            }
        }

        stage('Generate SBOM') {
            steps {
                // This command will now succeed because the tool was installed
                // in the previous stage.
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
            // This will now successfully archive the sbom.json file.
            archiveArtifacts artifacts: 'sbom.json', fingerprint: true
        }
    }
}