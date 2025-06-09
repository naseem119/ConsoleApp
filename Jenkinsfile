pipeline {
    agent any

    environment {sad
        PATH = "${env.PATH};${env.USERPROFILE}\\.dotnet\\tools"
    }

    stages {
        stage('Restore') {
            steps {
                bat 'dotnet restore DemoApp/DemoApp.csproj'
            }
        }

        stage('Build') {
            steps {
                bat 'dotnet build DemoApp/DemoApp.csproj --no-restore'
            }
        }

        stage('Generate SBOM') {
            steps {
                bat 'cyclonedx dotnet -p DemoApp/DemoApp.csproj -o sbom.json'
            }
        }

        stage('Validate SBOM') {
            steps {
                bat '''
                if exist sbom.json (
                    echo SBOM file created successfully.
                    type sbom.json
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
            archiveArtifacts artifacts: 'sbom.json', fingerprint: true
        }
    }
}
