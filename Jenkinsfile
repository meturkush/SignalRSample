pipeline {
    agent any

    stages {
        stage('Build') {
            steps {
                bat 'dotnet build SignalRSample.sln -c Release'
            }
        }

        stage('Test') {
            steps {
                bat 'dotnet test SignalRSample.Tests\\SignalRSample.Tests.csproj -c Release --no-build'
            }
        }

        stage('Stop App') {
            steps {
                bat 'powershell -Command "Stop-Service -Name SignalRSampleService -ErrorAction SilentlyContinue"'
            }
        }

        stage('Deploy') {
            steps {
                bat 'dotnet publish SignalRSample\\SignalRSample.csproj -c Release -o C:\\Deploy\\SignalRSample --no-build'
            }
        }

        stage('Start App') {
            steps {
                bat 'powershell -Command "Start-Service -Name SignalRSampleService"'
            }
        }
    }
}
