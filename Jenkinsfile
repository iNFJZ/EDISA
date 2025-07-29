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
        
        stage('Pre-Cleanup: Stop and Remove All Containers') {
            steps {
                script {
                    // Stop all running containers
                    sh 'docker stop $(docker ps -aq) || true'
                    // Remove all containers
                    sh 'docker rm $(docker ps -aq) || true'
                    // Remove all unused networks
                    sh 'docker network prune -f || true'
                    // Remove all unused volumes
                    sh 'docker volume prune -f || true'
                    echo "✅ Pre-cleanup completed: All containers, networks, and volumes removed"
                }
            }
        }
        
        stage('Build Docker Images') {
            steps {
                sh 'docker-compose build --no-cache'
            }
        }
        
        stage('Deploy Containers') {
            steps {
                sh 'docker-compose up --build -d'
                // Wait for services to be ready
                sleep 30
            }
        }
        
        stage('Health Check') {
            steps {
                script {
                    // Wait for services to be healthy
                    timeout(time: 5, unit: 'MINUTES') {
                        sh '''
                            echo "Checking service health..."
                            docker-compose ps
                            
                            # Check if all services are running
                            if docker-compose ps | grep -q "Exit"; then
                                echo "❌ Some services failed to start"
                                docker-compose logs
                                exit 1
                            fi
                            
                            echo "✅ All services are running successfully"
                        '''
                    }
                }
            }
        }
    }
    
    post {
        always {
            script {
                echo "🧹 Starting post-cleanup process..."
                
                // Stop all containers
                sh 'docker-compose down -v --remove-orphans || true'
                
                // Stop all running containers
                sh 'docker stop $(docker ps -aq) || true'
                
                // Remove all containers
                sh 'docker rm $(docker ps -aq) || true'
                
                // Remove all unused networks
                sh 'docker network prune -f || true'
                
                // Remove all unused volumes
                sh 'docker volume prune -f || true'
                
                // Remove dangling images
                sh 'docker image prune -f || true'
                
                echo "✅ Post-cleanup completed: All containers, networks, volumes, and dangling images removed"
            }
        }
        
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
                    <p><b>Cleanup Status:</b> All containers, networks, volumes, and dangling images have been removed.</p>
                """,
                mimeType: 'text/html',
                to: "${env.EMAIL_RECIPIENTS}"
            )
        }
        
        failure {
            script {
                echo "🚨 Build failed, performing emergency cleanup..."
                
                // Emergency cleanup even on failure
                sh 'docker-compose down -v --remove-orphans || true'
                sh 'docker stop $(docker ps -aq) || true'
                sh 'docker rm $(docker ps -aq) || true'
                sh 'docker network prune -f || true'
                sh 'docker volume prune -f || true'
                sh 'docker image prune -f || true'
                
                echo "✅ Emergency cleanup completed"
            }
            
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
                    <p><b>Emergency Cleanup:</b> All containers, networks, volumes, and dangling images have been removed.</p>
                    <pre>
                    ${currentBuild.rawBuild.getLog(50).join('\n')}
                    </pre>
                """,
                mimeType: 'text/html',
                to: "${env.EMAIL_RECIPIENTS}"
            )
        }
        
        cleanup {
            script {
                echo "🧹 Final cleanup in progress..."
                
                // Final cleanup to ensure nothing is left
                sh 'docker-compose down -v --remove-orphans || true'
                sh 'docker stop $(docker ps -aq) || true'
                sh 'docker rm $(docker ps -aq) || true'
                sh 'docker network prune -f || true'
                sh 'docker volume prune -f || true'
                sh 'docker image prune -f || true'
                
                echo "✅ Final cleanup completed"
            }
        }
    }
} 