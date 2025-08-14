using Microsoft.EntityFrameworkCore;
using EmailService.Models;

namespace EmailService.Data
{
    public class EmailServiceDbContext : DbContext
    {
        public EmailServiceDbContext(DbContextOptions<EmailServiceDbContext> options) : base(options)
        {
        }

        public DbSet<EmailTemplate> EmailTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EmailTemplate>()
                .HasIndex(et => new { et.Name, et.Language })
                .IsUnique();

            SeedEmailTemplates(modelBuilder);
        }

        private void SeedEmailTemplates(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 1,
                Name = "verify-email",
                Language = "en",
                Subject = "Verify Your Email Address",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Verify Your Email</title>
</head>
<body>
    <h2>Hello {{Username}},</h2>
    <p>Please verify your email address by clicking the link below:</p>
    <p><a href=""{{VerifyLink}}"">Verify Email</a></p>
    <p>If you didn't create an account, please ignore this email.</p>
</body>
</html>",
                Description = "Template for email verification",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 2,
                Name = "verify-email",
                Language = "vi",
                Subject = "Xác thực địa chỉ email của bạn",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Xác thực Email</title>
</head>
<body>
    <h2>Xin chào {{Username}},</h2>
    <p>Vui lòng xác thực địa chỉ email của bạn bằng cách nhấp vào liên kết dưới đây:</p>
    <p><a href=""{{VerifyLink}}"">Xác thực Email</a></p>
    <p>Nếu bạn không tạo tài khoản, vui lòng bỏ qua email này.</p>
</body>
</html>",
                Description = "Template cho xác thực email",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 3,
                Name = "verify-email",
                Language = "ja",
                Subject = "メールアドレスの確認",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>メール確認</title>
</head>
<body>
    <h2>{{Username}}様、</h2>
    <p>以下のリンクをクリックしてメールアドレスを確認してください：</p>
    <p><a href=""{{VerifyLink}}"">メール確認</a></p>
    <p>アカウントを作成していない場合は、このメールを無視してください。</p>
</body>
</html>",
                Description = "メール確認用テンプレート",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 4,
                Name = "reset-password",
                Language = "en",
                Subject = "Reset Your Password",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Reset Password</title>
</head>
<body>
    <h2>Hello {{Username}},</h2>
    <p>You requested a password reset for your account.</p>
    <p>Click the link below to reset your password:</p>
    <p><a href=""{{ResetLink}}"">Reset Password</a></p>
    <p>This link will expire in {{ExpiryMinutes}} minutes.</p>
    <p>If you didn't request this, please ignore this email.</p>
</body>
</html>",
                Description = "Template for password reset",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 5,
                Name = "reset-password",
                Language = "vi",
                Subject = "Đặt lại mật khẩu của bạn",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Đặt lại mật khẩu</title>
</head>
<body>
    <h2>Xin chào {{Username}},</h2>
    <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình.</p>
    <p>Nhấp vào liên kết dưới đây để đặt lại mật khẩu:</p>
    <p><a href=""{{ResetLink}}"">Đặt lại mật khẩu</a></p>
    <p>Liên kết này sẽ hết hạn sau {{ExpiryMinutes}} phút.</p>
    <p>Nếu bạn không yêu cầu điều này, vui lòng bỏ qua email này.</p>
</body>
</html>",
                Description = "Template cho đặt lại mật khẩu",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 6,
                Name = "reset-password",
                Language = "ja",
                Subject = "パスワードのリセット",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>パスワードリセット</title>
</head>
<body>
    <h2>{{Username}}様、</h2>
    <p>アカウントのパスワードリセットをリクエストしました。</p>
    <p>以下のリンクをクリックしてパスワードをリセットしてください：</p>
    <p><a href=""{{ResetLink}}"">パスワードリセット</a></p>
    <p>このリンクは{{ExpiryMinutes}}分後に期限切れになります。</p>
    <p>リクエストしていない場合は、このメールを無視してください。</p>
</body>
</html>",
                Description = "パスワードリセット用テンプレート",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 7,
                Name = "change-password",
                Language = "en",
                Subject = "Password Changed Successfully",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Password Changed</title>
</head>
<body>
    <h2>Hello {{Username}},</h2>
    <p>Your password has been changed successfully.</p>
    <p>If you didn't make this change, please contact support immediately.</p>
</body>
</html>",
                Description = "Template for password change confirmation",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 8,
                Name = "change-password",
                Language = "vi",
                Subject = "Mật khẩu đã được thay đổi thành công",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Mật khẩu đã thay đổi</title>
</head>
<body>
    <h2>Xin chào {{Username}},</h2>
    <p>Mật khẩu của bạn đã được thay đổi thành công.</p>
    <p>Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ hỗ trợ ngay lập tức.</p>
</body>
</html>",
                Description = "Template cho xác nhận thay đổi mật khẩu",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 9,
                Name = "change-password",
                Language = "ja",
                Subject = "パスワードが正常に変更されました",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>パスワード変更</title>
</head>
<body>
    <h2>{{Username}}様、</h2>
    <p>パスワードが正常に変更されました。</p>
    <p>この変更を行っていない場合は、すぐにサポートにお問い合わせください。</p>
</body>
</html>",
                Description = "パスワード変更確認用テンプレート",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 10,
                Name = "deactivate-account",
                Language = "en",
                Subject = "Account Deactivated",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Account Deactivated</title>
</head>
<body>
    <h2>Hello {{Username}},</h2>
    <p>Your account has been deactivated.</p>
    <p>If you have any questions, please contact support.</p>
</body>
</html>",
                Description = "Template for account deactivation",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 11,
                Name = "deactivate-account",
                Language = "vi",
                Subject = "Tài khoản đã bị vô hiệu hóa",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Tài khoản đã bị vô hiệu hóa</title>
</head>
<body>
    <h2>Xin chào {{Username}},</h2>
    <p>Tài khoản của bạn đã bị vô hiệu hóa.</p>
    <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ hỗ trợ.</p>
</body>
</html>",
                Description = "Template cho vô hiệu hóa tài khoản",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 12,
                Name = "deactivate-account",
                Language = "ja",
                Subject = "アカウントが無効化されました",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>アカウント無効化</title>
</head>
<body>
    <h2>{{Username}}様、</h2>
    <p>アカウントが無効化されました。</p>
    <p>ご不明な点がございましたら、サポートにお問い合わせください。</p>
</body>
</html>",
                Description = "アカウント無効化用テンプレート",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 13,
                Name = "register-google",
                Language = "en",
                Subject = "Welcome to Our Service",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Welcome</title>
</head>
<body>
    <h2>Welcome {{Username}}!</h2>
    <p>Thank you for registering with Google.</p>
    <p>Your account has been created successfully.</p>
</body>
</html>",
                Description = "Template for Google registration",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 14,
                Name = "register-google",
                Language = "vi",
                Subject = "Chào mừng đến với dịch vụ của chúng tôi",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Chào mừng</title>
</head>
<body>
    <h2>Chào mừng {{Username}}!</h2>
    <p>Cảm ơn bạn đã đăng ký bằng Google.</p>
    <p>Tài khoản của bạn đã được tạo thành công.</p>
</body>
</html>",
                Description = "Template cho đăng ký Google",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 15,
                Name = "register-google",
                Language = "ja",
                Subject = "サービスへようこそ",
            Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>ようこそ</title>
</head>
<body>
    <h2>{{Username}}様、ようこそ！</h2>
    <p>Googleでご登録いただき、ありがとうございます。</p>
    <p>アカウントが正常に作成されました。</p>
</body>
</html>",
                Description = "Google登録用テンプレート",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 16,
                Name = "restore-account",
                Language = "en",
                Subject = "Account Restored",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Account Restored</title>
</head>
<body>
    <h2>Hello {{Username}},</h2>
    <p>Your account has been restored successfully.</p>
    <p>Restored at: {{RestoredAt}}</p>
    <p>Reason: {{Reason}}</p>
    <p><a href=""{{LoginUrl}}"">Login to your account</a></p>
</body>
</html>",
                Description = "Template for account restoration",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 17,
                Name = "restore-account",
                Language = "vi",
                Subject = "Tài khoản đã được khôi phục",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Tài khoản đã được khôi phục</title>
</head>
<body>
    <h2>Xin chào {{Username}},</h2>
    <p>Tài khoản của bạn đã được khôi phục thành công.</p>
    <p>Khôi phục lúc: {{RestoredAt}}</p>
    <p>Lý do: {{Reason}}</p>
    <p><a href=""{{LoginUrl}}"">Đăng nhập vào tài khoản của bạn</a></p>
</body>
</html>",
                Description = "Template cho khôi phục tài khoản",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
    
            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 18,
                Name = "restore-account",
                Language = "ja",
                Subject = "アカウントが復元されました",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>アカウント復元</title>
</head>
<body>
    <h2>{{Username}}様、</h2>
    <p>アカウントが正常に復元されました。</p>
    <p>復元日時: {{RestoredAt}}</p>
    <p>理由: {{Reason}}</p>
    <p><a href=""{{LoginUrl}}"">アカウントにログイン</a></p>
</body>
</html>",
                Description = "アカウント復元用テンプレート",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            // File Upload Templates
            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 19,
                Name = "file-upload",
                Language = "en",
                Subject = "File Uploaded Successfully",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>File Uploaded</title>
</head>
<body>
    <h2>Hello {{Username}},</h2>
    <p>Your file has been uploaded successfully.</p>
    <p><strong>File Details:</strong></p>
    <ul>
        <li><strong>File Name:</strong> {{FileName}}</li>
        <li><strong>File Size:</strong> {{FileSize}}</li>
        <li><strong>Upload Time:</strong> {{UploadTime}}</li>
        <li><strong>IP Address:</strong> {{IpAddress}}</li>
    </ul>
    <p>Thank you for using our service!</p>
</body>
</html>",
                Description = "Template for file upload notification",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 20,
                Name = "file-upload",
                Language = "vi",
                Subject = "Tệp đã được tải lên thành công",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Tệp đã được tải lên</title>
</head>
<body>
    <h2>Xin chào {{Username}},</h2>
    <p>Tệp của bạn đã được tải lên thành công.</p>
    <p><strong>Chi tiết tệp:</strong></p>
    <ul>
        <li><strong>Tên tệp:</strong> {{FileName}}</li>
        <li><strong>Kích thước:</strong> {{FileSize}}</li>
        <li><strong>Thời gian tải lên:</strong> {{UploadTime}}</li>
        <li><strong>Địa chỉ IP:</strong> {{IpAddress}}</li>
    </ul>
    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
</body>
</html>",
                Description = "Template cho thông báo tải tệp lên",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 21,
                Name = "file-upload",
                Language = "ja",
                Subject = "ファイルのアップロードが完了しました",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>ファイルアップロード完了</title>
</head>
<body>
    <h2>{{Username}}様、</h2>
    <p>ファイルのアップロードが正常に完了しました。</p>
    <p><strong>ファイル詳細:</strong></p>
    <ul>
        <li><strong>ファイル名:</strong> {{FileName}}</li>
        <li><strong>ファイルサイズ:</strong> {{FileSize}}</li>
        <li><strong>アップロード時刻:</strong> {{UploadTime}}</li>
        <li><strong>IPアドレス:</strong> {{IpAddress}}</li>
    </ul>
    <p>サービスをご利用いただき、ありがとうございます！</p>
</body>
</html>",
                Description = "ファイルアップロード通知用テンプレート",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            // File Download Templates
            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 22,
                Name = "file-download",
                Language = "en",
                Subject = "File Downloaded Successfully",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>File Downloaded</title>
</head>
<body>
    <h2>Hello {{Username}},</h2>
    <p>Your file has been downloaded successfully.</p>
    <p><strong>File Details:</strong></p>
    <ul>
        <li><strong>File Name:</strong> {{FileName}}</li>
        <li><strong>File Size:</strong> {{FileSize}}</li>
        <li><strong>Download Time:</strong> {{DownloadTime}}</li>
        <li><strong>IP Address:</strong> {{IpAddress}}</li>
    </ul>
    <p>Thank you for using our service!</p>
</body>
</html>",
                Description = "Template for file download notification",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 23,
                Name = "file-download",
                Language = "vi",
                Subject = "Tệp đã được tải xuống thành công",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Tệp đã được tải xuống</title>
</head>
<body>
    <h2>Xin chào {{Username}},</h2>
    <p>Tệp của bạn đã được tải xuống thành công.</p>
    <p><strong>Chi tiết tệp:</strong></p>
    <ul>
        <li><strong>Tên tệp:</strong> {{FileName}}</li>
        <li><strong>Kích thước:</strong> {{FileSize}}</li>
        <li><strong>Thời gian tải xuống:</strong> {{DownloadTime}}</li>
        <li><strong>Địa chỉ IP:</strong> {{IpAddress}}</li>
    </ul>
    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
</body>
</html>",
                Description = "Template cho thông báo tải tệp xuống",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 24,
                Name = "file-download",
                Language = "ja",
                Subject = "ファイルのダウンロードが完了しました",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>ファイルダウンロード完了</title>
</head>
<body>
    <h2>{{Username}}様、</h2>
    <p>ファイルのダウンロードが正常に完了しました。</p>
    <p><strong>ファイル詳細:</strong></p>
    <ul>
        <li><strong>ファイル名:</strong> {{FileName}}</li>
        <li><strong>ファイルサイズ:</strong> {{FileSize}}</li>
        <li><strong>ダウンロード時刻:</strong> {{DownloadTime}}</li>
        <li><strong>IPアドレス:</strong> {{IpAddress}}</li>
    </ul>
    <p>サービスをご利用いただき、ありがとうございます！</p>
</body>
</html>",
                Description = "ファイルダウンロード通知用テンプレート",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            // File Delete Templates
            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 25,
                Name = "file-delete",
                Language = "en",
                Subject = "File Deleted Successfully",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>File Deleted</title>
</head>
<body>
    <h2>Hello {{Username}},</h2>
    <p>Your file has been deleted successfully.</p>
    <p><strong>File Details:</strong></p>
    <ul>
        <li><strong>File Name:</strong> {{FileName}}</li>
        <li><strong>File Size:</strong> {{FileSize}}</li>
        <li><strong>Delete Time:</strong> {{DeleteTime}}</li>
        <li><strong>IP Address:</strong> {{IpAddress}}</li>
    </ul>
    <p>Thank you for using our service!</p>
</body>
</html>",
                Description = "Template for file delete notification",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 26,
                Name = "file-delete",
                Language = "vi",
                Subject = "Tệp đã được xóa thành công",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Tệp đã được xóa</title>
</head>
<body>
    <h2>Xin chào {{Username}},</h2>
    <p>Tệp của bạn đã được xóa thành công.</p>
    <p><strong>Chi tiết tệp:</strong></p>
    <ul>
        <li><strong>Tên tệp:</strong> {{FileName}}</li>
        <li><strong>Kích thước:</strong> {{FileSize}}</li>
        <li><strong>Thời gian xóa:</strong> {{DeleteTime}}</li>
        <li><strong>Địa chỉ IP:</strong> {{IpAddress}}</li>
    </ul>
    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
</body>
</html>",
                Description = "Template cho thông báo xóa tệp",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<EmailTemplate>().HasData(new EmailTemplate
            {
                Id = 27,
                Name = "file-delete",
                Language = "ja",
                Subject = "ファイルの削除が完了しました",
                Body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>ファイル削除完了</title>
</head>
<body>
    <h2>{{Username}}様、</h2>
    <p>ファイルの削除が正常に完了しました。</p>
    <p><strong>ファイル詳細:</strong></p>
    <ul>
        <li><strong>ファイル名:</strong> {{FileName}}</li>
        <li><strong>ファイルサイズ:</strong> {{FileSize}}</li>
        <li><strong>削除時刻:</strong> {{DeleteTime}}</li>
        <li><strong>IPアドレス:</strong> {{IpAddress}}</li>
    </ul>
    <p>サービスをご利用いただき、ありがとうございます！</p>
</body>
</html>",
                Description = "ファイル削除通知用テンプレート",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
