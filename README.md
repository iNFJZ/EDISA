# EDISA Microservice System

Hệ thống microservice .NET 8 gồm các service:
- **AuthService**: Xác thực, phân quyền, quản lý user, JWT, Redis session.
- **FileService**: Upload/download/list/delete file với MinIO, gửi event qua RabbitMQ.
- **GatewayApi**: API Gateway dùng Ocelot.
- **EmailService**: Nhận event từ RabbitMQ, gửi email notification qua Gmail SMTP.
- **UserService**: Quản trị người dùng, tích hợp cache Redis, gửi notification.
- **NotificationService**: REST + SignalR Hub đẩy thông báo realtime.
- **GrpcGreeter**: gRPC server demo.
- **WorkerService**: Worker background consume message.
- **Frontend**: Giao diện người dùng (Nginx), mount LanguageFiles.

## 🏗️ Kiến trúc tổng thể

```
Client <-> Frontend <-> GatewayApi (Ocelot)
                |
    ┌────────────┼────────────┐
    |            |            |
AuthService  FileService  EmailService
    |            |            |
PostgreSQL    MinIO      RabbitMQ
    |            |            |
   Redis      MinIO UI   RabbitMQ UI
```

## 🚀 Tính năng nổi bật

### AuthService
- Đăng ký, đăng nhập, xác thực JWT, quản lý session với Redis.
- Đăng ký thành công sẽ gửi event qua RabbitMQ để EmailService gửi mail chào mừng.

### FileService
- Upload, download, list, delete file với MinIO.
- Mỗi thao tác upload/download/delete sẽ gửi event qua RabbitMQ để EmailService gửi mail thông báo.
- Tất cả endpoint đều yêu cầu JWT hợp lệ.

### EmailService
- Consume event từ RabbitMQ.
- Gửi email notification qua Gmail SMTP (hỗ trợ app password).

### GatewayApi
- Route request đến các service qua Ocelot.
- Hỗ trợ load balancing, request aggregation.

### Frontend
- Giao diện quản trị, chia module rõ ràng (html, js, css, assets)
- Build và deploy tự động qua Jenkins + Docker Compose
- Sử dụng Nginx làm reverse proxy
- Tích hợp CI/CD tự động, đồng bộ codebase, kiểm tra lỗi tự động

## 🛠️ Công nghệ sử dụng

- .NET 8, Entity Framework Core, AutoMapper, Serilog, OpenTelemetry (extension)
- PostgreSQL, Redis, MinIO, RabbitMQ, Elasticsearch + Kibana
- Ocelot (Gateway), JWT, SignalR
- Docker, Docker Compose, Kubernetes (Kustomize, Ingress-NGINX, HPA)
- Jenkins CI/CD, ngrok (webhook), Nginx (Frontend)

## 🚀 Quick Start

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK
- Tùy chọn cho Kubernetes: `kubectl`, Minikube hoặc kind, Ingress-NGINX, metrics-server
- Jenkins (tùy chọn, chạy qua Docker) + ngrok (nếu dùng webhook GitHub)

### 1) Clone repository
```bash
git clone <repository-url>
cd EDISA
```

### 2) Chạy nhanh bằng Docker Compose (local dev)
```bash
docker compose up --build
# hoặc chạy nền
docker compose up -d
```

Tắt hệ thống:
```bash
docker compose down
# Xóa luôn volume (xóa sạch data):
docker compose down -v
```

URL khi chạy Docker Compose (mặc định):
- RabbitMQ UI: http://localhost:15672 (guest/guest)
- MinIO UI: http://localhost:9001 (minio/minio123)
- Elasticsearch: http://localhost:9200, Kibana: http://localhost:5601
- AuthService Swagger: http://localhost:5001/swagger
- FileService Swagger: http://localhost:5002/swagger
- UserService Swagger: http://localhost:5005/swagger
- NotificationService Swagger: http://localhost:5006/swagger
- GatewayApi: http://localhost:5050
- Frontend: http://localhost:8080

### 3) Chạy bằng Kubernetes (khuyến nghị)

1. Khởi tạo cluster và Ingress
- Minikube:
```bash
minikube start --driver=docker
minikube addons enable ingress
minikube addons enable metrics-server
```
- kind (ví dụ):
```bash
kind create cluster
# Cài ingress-nginx theo hướng dẫn chính thức (nếu chưa có)
```

2. Build image và nạp vào cluster (Mac M-series nên build linux/amd64)
```bash
docker build --platform linux/amd64 -t edisa:latest .
docker build --platform linux/amd64 -t edisa-frontend:latest ./Frontend

# Minikube
minikube image load edisa:latest
minikube image load edisa-frontend:latest

# kind (nếu dùng kind)
# kind load docker-image edisa:latest
# kind load docker-image edisa-frontend:latest
```

3. Apply K8s manifests
```bash
kubectl apply -k k8s
kubectl get pods -n edisa
```

4. Trỏ domain `edisa.local` về Ingress
- Minikube:
```bash
echo "$(minikube ip) edisa.local" | sudo tee -a /etc/hosts
```
- kind: trỏ IP của ingress-nginx vào `/etc/hosts` (hoặc dùng `kubectl port-forward`).

5. Truy cập qua Ingress
- Frontend: http://edisa.local/
- API Gateway: http://edisa.local/api
- Auth: http://edisa.local/auth
- Files: http://edisa.local/files
- Users: http://edisa.local/users
- Emails: http://edisa.local/emails
- Notifications: http://edisa.local/notifications, SignalR: `ws(s)://edisa.local/notificationhub`
- MinIO: http://edisa.local/minio, Console: http://edisa.local/minio-console
- Kibana: http://edisa.local/kibana
- RabbitMQ UI: http://edisa.local/rabbitmq
- gRPC demo: http://edisa.local/grpc (service port 5219 trong cluster)

6. Teardown
```bash
kubectl delete -k k8s
```

## 📡 API Endpoints (tổng quan)

### AuthService (http://localhost:5001)
- `POST /api/auth/register` - Đăng ký user
- `POST /api/auth/login` - Đăng nhập, nhận JWT
- `POST /api/auth/logout` - Đăng xuất
- `POST /api/auth/validate` - Kiểm tra token hợp lệ
- `GET /api/auth/sessions` - Danh sách phiên đăng nhập
- `DELETE /api/auth/sessions/{sessionId}` - Xóa phiên
- `POST /api/auth/forgot-password` - Quên mật khẩu (gửi email reset)
- `POST /api/auth/reset-password` - Đặt lại mật khẩu (dùng token)
- `POST /api/auth/change-password` - Đổi mật khẩu (yêu cầu đăng nhập)

### FileService (http://localhost:5002)
- `POST /api/file/upload` - Upload file (multipart/form-data)
- `GET /api/file/download/{fileName}` - Download file
- `DELETE /api/file/delete/{fileName}` - Xóa file
- `GET /api/file/list` - Liệt kê file

### GatewayApi
- Upstream: `/api/Auth/*`, `/api/File/*`, `/api/User/*`, `/api/Notification/*`, `/api/Email/*`
- Downstream: theo `GatewayApi/ocelot.json` tới các service cùng namespace `edisa`.

## 📨 Email Notification (Event-driven)
- Đăng ký, upload, download, delete file gửi event qua RabbitMQ.
- EmailService consume event, gửi email với nội dung động.

## 🗄️ Cấu hình môi trường

### Docker Compose (mặc định trong `docker-compose.yml`)
- PostgreSQL: `postgres:5432`, user: `postgres`, pass: `123456`
- Redis: `redis:6379`
- MinIO: `minio:9000` (access: `minio`, secret: `minio123`, bucket: `mybucket`)
- RabbitMQ: `rabbitmq:5672` (guest/guest), UI: `15672`
- Elasticsearch: `elasticsearch:9200`, Kibana: `kibana:5601`

### Kubernetes (`k8s/`)
- `k8s/configmaps.yaml` (ConfigMap `edisa-config`) chứa: `postgres_host/port`, `redis_host/port`, `rabbitmq_*`, `minio_*`, `elasticsearch_url`, các URL service, `aspnetcore_environment`, `aspnetcore_urls`, JWT config, CORS.
- `k8s/secrets.yaml` (Secret `edisa-secrets`) chứa: `postgres_password`, `minio_password`, `rabbitmq_password`, `jwt_secret`, `email_password`, `google_client_id/secret`, `hunter_api_key`. Vui lòng thay thế giá trị thật (base64) trước khi deploy production.
- Ingress: `k8s/ingress.yaml` khai báo host `edisa.local` và routing tới các Service nội bộ.
- Lưu ý Storage: `k8s/storage.yaml` dùng `hostPath` + `local-storage` (phục vụ dev). Production nên dùng StorageClass phù hợp cloud.

### Cấu hình ứng dụng
- Mỗi service có `appsettings.json` và `appsettings.Development.json`. Biến môi trường override từ Docker/K8s.
- GatewayApi routing tại `GatewayApi/ocelot.json`.

## 🧪 Testing

```bash
dotnet test
```

## 🧬 Database & Migrations
- Các service dùng EF Core tự động chạy `Database.Migrate()` khi khởi động (Auth, User, Email, Notification).
- Tạo migration (ví dụ cho AuthService):
```bash
cd AuthService
dotnet ef migrations add <Name> --project AuthService.csproj
dotnet ef database update --project AuthService.csproj
```
- Seed dữ liệu mẫu (tùy chọn): dùng `sample_data.sql` vào PostgreSQL.
  - Docker Compose: `docker exec -i edisa psql -U postgres -d postgres < sample_data.sql`
  - K8s: `kubectl -n edisa exec -it deploy/postgres -- bash -lc "psql -U $POSTGRES_USER -d $POSTGRES_DB"` rồi chạy nội dung file.

## 📝 CI/CD & DevOps

- Jenkins pipeline: xem `Jenkinsfile`. Pipeline dọn dẹp container conflict, build và `docker compose up -d` theo commit mới.
- Webhook: dùng ngrok để expose Jenkins nếu chạy local (ví dụ: `ngrok http 9090`), trỏ GitHub webhook về URL ngrok.
- Tài liệu chi tiết: `k8s/COMPLETE_GUIDE.md`.

## 📊 Monitoring & UI

- RabbitMQ UI: Docker Compose `http://localhost:15672`, K8s qua Ingress `http://edisa.local/rabbitmq`
- MinIO UI: Docker Compose `http://localhost:9001`, K8s `http://edisa.local/minio-console`
- Swagger: theo từng service cổng local hoặc qua Ingress (Gateway proxy theo Ocelot)
- Kibana: Docker Compose `http://localhost:5601`, K8s `http://edisa.local/kibana`
- Jenkins: http://localhost:9090 (nếu chạy)

## 📂 Cấu trúc thư mục (rút gọn)

```
EDISA/
├── AuthService/               # API + gRPC, EF Migrations tự migrate
├── FileService/               # API + gRPC, MinIO, JWT
├── GatewayApi/                # Ocelot API Gateway (ocelot.json)
├── EmailService/              # Consume event, gửi email
├── UserService/               # Quản lý người dùng, Redis cache
├── NotificationService/       # REST + SignalR Hub
├── GrpcGreeter/               # gRPC demo
├── WorkerService/             # Worker background
├── Shared/                    # Thư viện dùng chung + LanguageFiles
├── Frontend/                  # Nginx static UI + nginx.conf
├── k8s/                       # Kustomize manifests (namespace, config, secrets, infra, services, ingress, hpa)
├── Postman_Collections/       # Sẵn sàng import để test API
├── docker-compose.yml         # Dev nhanh không cần K8s
├── Dockerfile                 # Multi-stage build cho toàn backend image `edisa`
├── README.md
└── Jenkinsfile                # CI/CD pipeline
```

## 📦 Postman Collections
- Thư mục `Postman_Collections/` có sẵn collection cho Auth, File, Gateway. Import vào Postman để test nhanh.

## ⚠️ Lưu ý
- Giá trị trong `k8s/secrets.yaml` là ví dụ (base64). Hãy thay bằng secrets thật trước khi publish/production.
- `k8s/storage.yaml` dùng `hostPath` (phù hợp dev). Production cần StorageClass/volume chuẩn.
- Health checks trong manifests tạm comment để pods khởi động ổn định lần đầu. Có thể bật lại khi image ổn định.

