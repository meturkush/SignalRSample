pipeline {
    agent any

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore & Build') {
            steps {
                bat 'dotnet build SignalRSample.sln --configuration Release'
            }
        }

        stage('Run NUnit Tests') {
            steps {
                bat 'dotnet test SignalRSample.sln --configuration Release --no-build --logger "console;verbosity=normal"'
            }
        }

        stage('Publish (Deploy)') {
            steps {
                bat 'dotnet publish SignalRSample/SignalRSample.csproj --configuration Release --no-build --output C:\\Deploy\\SignalRSample'
            }
        }
    }

    post {
        success {
            echo 'Build, 27 NUnit tests, and deployment to C:\\Deploy\\SignalRSample succeeded!'
        }
        failure {
            echo 'Pipeline failed. Check the Console Output logs.'
        }
    }
}
