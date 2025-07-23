pipeline {
    agent any

    environment {
        EMAIL_RECIPIENTS = 'ilubeos@gmail.com'
    }

    stages {
        stage('Checkout') {
            steps {
                git branch: 'master', url: 'https://github.com/iNFJZ/EDISA.git'
            }
        }
        stage('Build Docker Images') {
            steps {
                sh 'docker-compose build'
            }
        }
        stage('Cleanup All Old Containers') {
            steps {
                sh 'docker rm -f grpc-server minio auth-postgres redis rabbitmq email-service auth-service file-service user-service gateway-api frontend || true'
            }
        }
        stage('Deploy Containers') {
            steps {
                sh 'docker-compose down -v --remove-orphans || true'
                sh 'docker-compose up --build -d'
            }
        }
    }
    post {
        success {
            emailext (
                subject: "✅ [SUCCESS] Deploy project successfully",
                body: "Project has been deployed successfully on Jenkins server.",
                to: "${env.EMAIL_RECIPIENTS}"
            )
        }
        failure {
            emailext (
                subject: "❌ [FAILURE] Deploy project failed",
                body: "The deployment process encountered an error. Please check the Jenkins build log.",
                to: "${env.EMAIL_RECIPIENTS}"
            )
        }
    }
} 