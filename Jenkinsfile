pipeline {
    agent any

        environment {
        PATH = "C:\\Users\\HP\\AppData\\Local\\Programs\\DockerDesktop\\resources\\bin;${env.PATH}"
    }


    stages {
        stage('Build') {
            steps {
                bat 'dotnet build SignalRSample.sln -c Release'
            }
        }

        stage('Test') {
            steps {
                bat 'dotnet test SignalRSample.Tests\\SignalRSample.Tests.csproj -c Release --no-build --logger "nunit;LogFilePath=TestResults\\nunit-results.xml"'
            }
            post {
                always {
                    nunit testResultsPattern: '**/TestResults/nunit-results.xml'
                }
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

        stage('Docker Build & Run') {
            steps {
                bat 'docker build -t signalrsample:latest .'
                bat 'docker rm -f signalrsample-container || exit /b 0'
                bat 'docker run -d --name signalrsample-container -p 5050:80 signalrsample:latest'
            }
        }
    }
}
