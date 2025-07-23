# Microservice System

Hệ thống microservice .NET 8 gồm các service:
- **AuthService**: Xác thực, phân quyền, quản lý user, JWT, Redis session.
- **FileService**: Upload/download/list/delete file với MinIO, gửi event qua RabbitMQ.
- **GatewayApi**: API Gateway dùng Ocelot.
- **EmailService**: Nhận event từ RabbitMQ, gửi email notification qua Gmail SMTP.
- **Frontend**: Giao diện người dùng, quản lý qua Nginx, tích hợp CI/CD Jenkins.

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

- .NET 8, Entity Framework Core
- PostgreSQL, Redis, MinIO, RabbitMQ
- Ocelot, JWT, Docker Compose, Jenkins
- Gmail SMTP (app password) cho EmailService
- Nginx, jQuery, Toastr, ngrok (cho webhook Jenkins)

## 🚀 Quick Start

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK
- Jenkins (chạy qua Docker)
- Ngrok (nếu dùng webhook GitHub)

### 1. Clone repository
```bash
git clone <repository-url>
cd MicroserviceSystem
```

### 2. Build & chạy toàn bộ hệ thống
```bash
docker compose up --build
# hoặc chạy nền
docker compose up -d
```

### 3. Dừng hệ thống
```bash
docker compose down
# Xóa luôn volume (xóa sạch data)
docker compose down -v
```

### 4. Chạy Jenkins + ngrok cho CI/CD tự động
- Chạy Jenkins container mount docker.sock
- Chạy ngrok: `ngrok http 9090`
- Cấu hình webhook GitHub trỏ về: `https://<ngrok-id>.ngrok-free.app/github-webhook/`
- Push code lên GitHub, Jenkins sẽ tự động build/deploy

## 📡 API Endpoints

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

### GatewayApi (http://localhost:5050)
- `/api/auth/*` - Proxy đến AuthService
- `/api/file/*` - Proxy đến FileService

## 📨 Email Notification (Event-driven)
- Đăng ký, upload, download, delete file đều gửi event qua RabbitMQ.
- EmailService consume event, gửi email với nội dung động.

## 🗄️ Cấu hình môi trường

- **PostgreSQL**: auth_db, user: postgres, pass: 123456, port: 5432
- **Redis**: redis:6379
- **MinIO**: minio:9000, access: minio, secret: minio123, bucket: mybucket
- **RabbitMQ**: guest/guest, port: 5672, UI: 15672
- **Gmail SMTP**: cấu hình trong EmailService/appsettings.json

## 🧪 Testing

```bash
dotnet test
```

## 📝 Lưu ý thực tế & CI/CD

- **CI/CD Jenkins + Docker:**
  - Push code lên GitHub → webhook gửi về Jenkins (qua ngrok) → Jenkins tự động build, dọn dẹp container cũ, deploy lại toàn bộ hệ thống.
  - Jenkinsfile đã tự động dọn dẹp tất cả container có thể conflict:
    ```sh
    docker rm -f grpc-server minio auth-postgres redis rabbitmq email-service auth-service file-service user-service gateway-api frontend || true
    docker-compose down -v --remove-orphans || true
    ```
  - Build chỉ chạy khi có commit mới (SHA mới).
- **Quyền Docker cho Jenkins:**
  - Jenkins container phải mount đúng docker.sock và user jenkins phải thuộc group docker.
  - Nếu gặp lỗi permission denied, cần:
    - `chown root:docker /var/run/docker.sock`
    - `usermod -aG docker jenkins`
    - Restart Jenkins container
- **Webhook trả về 302/403:**
  - Kiểm tra CSRF Protection, quyền anonymous Jenkins, trigger job.
- **Quy tắc đồng bộ codebase:**
  - Mọi thay đổi phải đồng bộ ở tất cả các module liên quan.
  - Đảm bảo không còn code trùng lặp, dư thừa, hoặc lỗi logic.
  - Tách biệt rõ ràng HTML, CSS, JS.
  - Kiểm tra và fix syntax/linter error tự động.
  - Luôn test lại sau khi sửa, lặp lại fix/test cho đến khi hoàn toàn sạch lỗi.
- **Lỗi conflict container:**
  - Đã tự động dọn dẹp trong Jenkinsfile, nếu vẫn lỗi thì xóa thủ công như trên.
- **Jenkins không build khi push:**
  - Kiểm tra webhook GitHub, trigger job, quyền Jenkins, ngrok.

## 📊 Monitoring & UI

- **RabbitMQ UI**: http://localhost:15672 (guest/guest)
- **MinIO UI**: http://localhost:9001 (minio/minio123)
- **Swagger**: http://localhost:5001/swagger, http://localhost:5002/swagger
- **Jenkins**: http://localhost:9090
- **Frontend**: http://localhost:8080

## 📂 Cấu trúc thư mục

```
MicroserviceSystem/
├── AuthService/
├── FileService/
├── GatewayApi/
├── EmailService/
├── UserService/
├── GrpcGreeter/
├── Shared/
├── Frontend/
│   ├── html/
│   ├── js/
│   ├── css/
│   ├── assets/
│   ├── nginx.conf
│   └── Dockerfile
├── docker-compose.yml
├── Dockerfile
└── README.md
```
