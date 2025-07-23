pipeline {
    agent any

    environment {
        EMAIL_RECIPIENTS = 'dungnthe163811@fpt.edu.vn'
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
                subject: "✅ [SUCCESS] ${env.JOB_NAME} #${env.BUILD_NUMBER} deployed successfully",
                body: """
                    <h2>🎉 Deployment Successful!</h2>
                    <ul>
                      <li><b>Job:</b> ${env.JOB_NAME}</li>
                      <li><b>Build Number:</b> #${env.BUILD_NUMBER}</li>
                      <li><b>Time:</b> ${new Date().format('yyyy-MM-dd HH:mm:ss', TimeZone.getTimeZone('Asia/Ho_Chi_Minh'))}</li>
                      <li><b>Triggered by:</b> ${env.BUILD_USER ?: 'GitHub Webhook'}</li>
                      <li><b>Status:</b> <span style='color:green;'>SUCCESS</span></li>
                      <li><b>Details:</b> <a href='${env.BUILD_URL}console'>${env.BUILD_URL}console</a></li>
                    </ul>
                """,
                mimeType: 'text/html',
                to: "${env.EMAIL_RECIPIENTS}"
            )
        }
        failure {
            emailext (
                subject: "❌ [FAILURE] ${env.JOB_NAME} #${env.BUILD_NUMBER} deployment failed",
                body: """
                    <h2>🚨 Deployment Failed!</h2>
                    <ul>
                      <li><b>Job:</b> ${env.JOB_NAME}</li>
                      <li><b>Build Number:</b> #${env.BUILD_NUMBER}</li>
                      <li><b>Time:</b> ${new Date().format('yyyy-MM-dd HH:mm:ss', TimeZone.getTimeZone('Asia/Ho_Chi_Minh'))}</li>
                      <li><b>Triggered by:</b> ${env.BUILD_USER ?: 'GitHub Webhook'}</li>
                      <li><b>Status:</b> <span style='color:red;'>FAILURE</span></li>
                      <li><b>Error Details:</b> <a href='${env.BUILD_URL}console'>${env.BUILD_URL}console</a></li>
                    </ul>
                    <pre>
                    ${currentBuild.rawBuild.getLog(50).join('\n')}
                    </pre>
                """,
                mimeType: 'text/html',
                to: "${env.EMAIL_RECIPIENTS}"
            )
        }
    }
} 