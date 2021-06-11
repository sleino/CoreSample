Jenkinsfile (Declarative Pipeline)
pipeline {
    agent { docker { image 'python:3.5.1' } }
    stages {
        stage('build') {
            steps {
                dotnet build -c release smSimple.sln
            }
        }
    }
}
