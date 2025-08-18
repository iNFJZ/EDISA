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
<html lang=""en"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <!--[if mso]>
    <xml><o:officedocumentsettings><o:pixelsperinch>96</o:pixelsperinch></o:officedocumentsettings></xml>
  <![endif]-->
    <title>Welcome to EDISA</title>
    <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
    <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Please verify your email address</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Email verification"" lang=""en"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
  <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
    <!-- <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
      <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;"">
    </a> -->
  </td>
</tr>
              <tr>
                <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                    <tr>
                      <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">Hello</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                        <p class=""sm-leading-32"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                          Thank you for registering! 👋
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                          Please verify your email address by clicking the button below to join the EDISA community, start exploring resources, or showcase your products.
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                          If you didn't register for EDISA, please ignore this email or contact us at
                          <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>
                        </p>
                        <!-- <a href=""{{VerifyLink}}"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 24px; display: block; font-size: 16px; line-height: 100%; color: #7367f0; text-decoration: none;"">{{VerifyLink}}</a> -->
                        <table cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <tr>
                            <td style=""mso-line-height-rule: exactly; mso-padding-alt: 16px 24px; border-radius: 4px; background-color: #7367f0; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                              <a href=""{{VerifyLink}}"" style=""background-color: #7367f0; border-color: #7367f0; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: 600; font-size: 16px;"">Verify Email Now &rarr;</a>
                            </td>
                          </tr>
                        </table>
                        <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
  <tr>
    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
      <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
    </td>
  </tr>
</table>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
  Not sure why you received this email? Please
  <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">contact us</a>.
</p>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">Best regards,<br>The EDISA Team</p>
                      </td>
                    </tr>
                    <tr>
  <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
</tr>
<tr>
  <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
    <!-- <p align=""center"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 16px; cursor: default;"">
      ...
    </p> -->
    <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
      Your use of our services and website is subject to our
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Terms of Service</a> and
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Privacy Policy</a>.
    </p>
  </td>
</tr>
                  </table>
                </td>
              </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""vi"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <!--[if mso]>
    <xml><o:officedocumentsettings><o:pixelsperinch>96</o:pixelsperinch></o:officedocumentsettings></xml>
  <![endif]-->
    <title>Chào mừng đến với EDISA</title>
    <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
    <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Vui lòng xác thực địa chỉ email của bạn</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Xác thực email"" lang=""vi"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
  <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
    <!-- <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
      <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;"">
    </a> -->
  </td>
</tr>
              <tr>
                <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                    <tr>
                      <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">Xin chào</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                        <p class=""sm-leading-32"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                          Cảm ơn bạn đã đăng ký! 👋
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                          Vui lòng xác thực địa chỉ email của bạn bằng cách nhấn vào nút bên dưới để tham gia cộng đồng EDISA, bắt đầu khám phá tài nguyên hoặc giới thiệu sản phẩm của bạn.
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                          Nếu bạn không đăng ký EDISA, vui lòng bỏ qua email này hoặc liên hệ với chúng tôi qua địa chỉ
                          <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>
                        </p>
                        <!-- <a href=""{{VerifyLink}}"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 24px; display: block; font-size: 16px; line-height: 100%; color: #7367f0; text-decoration: none;"">{{VerifyLink}}</a> -->
                        <table cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <tr>
                            <td style=""mso-line-height-rule: exactly; mso-padding-alt: 16px 24px; border-radius: 4px; background-color: #7367f0; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                              <a href=""{{VerifyLink}}"" style=""background-color: #7367f0; border-color: #7367f0; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: 600; font-size: 16px;"">Xác thực email ngay &rarr;</a>
                            </td>
                          </tr>
                        </table>
                        <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
  <tr>
    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
      <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
    </td>
  </tr>
</table>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
  Không rõ vì sao bạn nhận được email này? Vui lòng
  <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">liên hệ với chúng tôi</a>.
</p>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">Trân trọng,<br>Đội ngũ EDISA</p>
                      </td>
                    </tr>
                    <tr>
  <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
</tr>
<tr>
  <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
    <!-- <p align=""center"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 16px; cursor: default;"">
      ...
    </p> -->
    <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
      Việc sử dụng dịch vụ và website của chúng tôi tuân theo
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Điều khoản sử dụng</a> và
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Chính sách bảo mật</a>.
    </p>
  </td>
</tr>
                  </table>
                </td>
              </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""ja"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <!--[if mso]>
    <xml><o:officedocumentsettings><o:pixelsperinch>96</o:pixelsperinch></o:officedocumentsettings></xml>
  <![endif]-->
    <title>EDISAへようこそ</title>
    <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
    <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">メールアドレスの確認をお願いします</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""メール確認"" lang=""ja"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
  <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
    <!-- <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
      <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;"">
    </a> -->
  </td>
</tr>
              <tr>
                <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                    <tr>
                      <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">こんにちは</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}様！</p>
                        <p class=""sm-leading-32"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                          ご登録ありがとうございます！👋
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                          下のボタンをクリックしてメールアドレスを確認し、EDISAコミュニティに参加し、リソースの探索を開始するか、製品を紹介してください。
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                          EDISAに登録していない場合は、このメールを無視するか、
                          <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>
                          までお問い合わせください。
                        </p>
                        <!-- <a href=""{{VerifyLink}}"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 24px; display: block; font-size: 16px; line-height: 100%; color: #7367f0; text-decoration: none;"">{{VerifyLink}}</a> -->
                        <table cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <tr>
                            <td style=""mso-line-height-rule: exactly; mso-padding-alt: 16px 24px; border-radius: 4px; background-color: #7367f0; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                              <a href=""{{VerifyLink}}"" style=""background-color: #7367f0; border-color: #7367f0; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: 600; font-size: 16px;"">今すぐメール確認 &rarr;</a>
                            </td>
                          </tr>
                        </table>
                        <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
  <tr>
    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
      <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
    </td>
  </tr>
</table>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
  このメールを受け取った理由がわからない場合は、
  <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">お問い合わせください</a>。
</p>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">よろしくお願いいたします、<br>EDISAチーム</p>
                      </td>
                    </tr>
                    <tr>
  <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
</tr>
<tr>
  <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
    <!-- <p align=""center"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 16px; cursor: default;"">
      ...
    </p> -->
    <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
      当社のサービスとウェブサイトのご利用は、
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">利用規約</a>と
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">プライバシーポリシー</a>に従います。
    </p>
  </td>
</tr>
                  </table>
                </td>
              </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""en"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <!--[if mso]>
    <xml><o:officedocumentsettings><o:pixelsperinch>96</o:pixelsperinch></o:officedocumentsettings></xml>
  <![endif]-->
    <title>Reset your Password</title>
    <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
    <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">A request to reset password was received from your EDISA Account</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Reset your Password"" lang=""en"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
  <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
    <!-- <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
      <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;"">
    </a> -->
  </td>
</tr>
              <tr>
                <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                    <tr>
                      <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">Hey</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                          A request to reset password was received from your
                          <span style=""font-weight: 600;"">EDISA</span> Account -
                          <a href=""mailto:{{Email}}"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">{{Email}}</a>
                          (ID: {{UserId}}) from the IP - <span style=""font-weight: 600;"">{{IpAddress}}</span> .
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">Clicking the below button to reset your password and login.</p>
                        <!-- <a href=""{{ResetLink}}"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 24px; display: block; font-size: 16px; line-height: 100%; color: #7367f0; text-decoration: none;"">{{ResetLink}}</a> -->
                        <table cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <tr>
                            <td style=""mso-line-height-rule: exactly; mso-padding-alt: 16px 24px; border-radius: 4px; background-color: #7367f0; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                              <a href=""{{ResetLink}}"" style=""background-color: #7367f0; border-color: #7367f0; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: 600; font-size: 16px;"">Reset Password &rarr;</a>
                            </td>
                          </tr>
                        </table>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-top: 24px; margin-bottom: 24px;"">
                          <span style=""font-weight: 600;"">Note:</span> This link is valid for {{ExpiryMinutes}} minutes from the time it was
                          sent to you and can be used to change your password only once.
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0;"">
                          If you did not request a password reset, please
                          contact us at
                          <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>
                        </p>
                        <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
  <tr>
    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
      <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
    </td>
  </tr>
</table>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
  Not sure why you received this email? Please
  <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">let us know</a>.
</p>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">Thanks, <br>The EDISA Team</p>
                      </td>
                    </tr>
                    <tr>
  <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
</tr>
<tr>
  <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
    <!-- <p align=""center"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 16px; cursor: default;"">
      <a href=""https://www.facebook.com/infjz"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238; text-decoration: none;""><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAABHNCSVQICAgIfAhkiAAAAr9JREFUWEfFmF9y2jAQxndt89zcoOQEJScoOUFyg6bPjAw+QcsJTKzhOdyg5ASlJ0g4QcMJGt69bGc9MmOM/0g2ne4rsvTTp0+7KxAcIo7jq8FgcHc4HMaIOASAKwAYmSleAeCdmd+YeT2dTp8dpga0Gay1HgPAFADubcbLGGZ+B4A1Ec2jKHpr+64RJI7jYRAEsQtA1YLMvCKiKIoigauMWpAkSR4AIEZEkb93iEK+799OJpM5wrOoBNFafwOA771Xr5iAmb+GYbgq/3QGorUWAAH5Z1EFcwIix4GITx0J9sz8iohya+RGZYGIn8vzVR3TEUSM6fv+SwdP7ADgQSm1qdpAncICQ0TXuYGPIFrrHx1uxzZN03HTbWg56rlSKvNiBmLyxE/XI0nTVHbUmCOaQIwqNzJHBpIkyRoR71xAmPk5DMPWBGdh/kel1AwlbQdB8McFwow9ylr8VmstcFNmHtn4TUpCGIbX2OOm3JYNajLxb9dNeZ53g1prSS5fXD8GgDOQJElmiCglwTXmosim6q5bzHQGYuGHumkfRZGXQim3WP845GIgzPxLQNhl9cLY/wOilLLqXcqbslE8V0TK8qc2VXqAtCqegdiatQuIw3XOzLowbWCjKF1AbEsHM0cCIplQCl5jMLP0rccgom252JkK/rEwTNoK6fQaQ2pWnxR/qVuzU0oN8+rbJbteCiSrWRmIg6mKEl8CZJ+m6VCOuNgYuapyCZDTxsioIu2A5JSi2ZpM1hdkq5TKX4mnLz3b62bo+oDIkYyK3d1Z2nboT7qC7D3PG5cfWpX1wxKmC0glhChcW8jMMYmB6zzjCiId/31ds932CBcDS7sv/wSUwxZkDwCL/NlQ536r0m7yjKTqGQB8sDTrjpkXRLRqevfkYFYgxV0sl8sREY2JaF2WWeqWPDd939/UvfrrFPkLB5ilt2wxKssAAAAASUVORK5CYII="" width=""17"" alt=""Facebook"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0; margin-right: 12px;""></a>
      &bull;
      <a href=""https://www.twitter.com/infjz"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238; text-decoration: none;""><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACMAAAAdCAYAAAAgqdWEAAAABHNCSVQICAgIfAhkiAAAAptJREFUSEu1V91x2kAQ3r3Te9xBSAWhAzsdkAoCz8zKVgUhFchIwzsdBFdgUkHsCmwqiHmFky6zmjtGPyehH7gZXrjb3W+//RXChU8Yhjee5/0AgAkA3LF6rfU7AGyllMv5fP5SNrlarcb8P9qLOI4nRLQZgo2VJknyjIg3dXq01o++7wcMWghxK4R4AIA/RLTIwBhv/gHAkoj4svNhIGma/m0pyOyMzdudUmocBMFHBoZZAYDfhtK17/uzlkqzZ+yMlPKtiZEafXshxN3hcPjwPG9kwSwA4GdOYKOUmjHaNqCiKHpAxLDN29KbLQBwSL8opWrBZEmHiDMiYoHGE0XRFhFvz71rYueUwPkwOQSYpSAIAq4I54njWPcE8iqEmNoKyycwG/vUoJS9Xx+Px6dy+AaA+ZZnPl/ajwBw39JDDh2De0/TlJ1YI+KopWz+mRuMqSouua89lPYVqYLhamAQWuuNEGIJAJ/7au8o5wbTszQ72i4+J6JTmvCNTeCR53lvgzR3F34lItuFM+kTsoG9ojsUx+g5gTHz6Vx59zFaJ/O9PJgLMTPDjif3tRN4R0SVVlAAwy4Yhri6plcE9YtXhjJlFTBRFG0Q0XbibDm68NnzUHQN4QqYOI7LE/zCWMDJSqGarEUTJm731+jElXLOe1phxuaNlJLD1XctcLGZLVKuHdg+doKxl2a14LBdgqVKKZ9NYJdLYRiOpJS8aPeZzLyozXzfX59LvkZmTP7ca615razd+JuMtAVSm8D8CYGIXNbTviAAYCeEmDTlSCVM5ltnwb1Faz0eYDyve6mUWrRd6AsJfKGuuwcA3pcZRO2+3BRSV9Pjbyj+8Xg/V0UZAK31NkmSTVcmOlWTYaywc1gFSqmXocbLYP4DC8Yg48wi1iQAAAAASUVORK5CYII="" width=""17"" alt=""Twitter"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0; margin-right: 12px;""></a>
      &bull;
      <a href=""https://www.instagram.com/infjz"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238; text-decoration: none;""><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAABHNCSVQICAgIfAhkiAAAA2VJREFUWEe9V0FW2zAQ1djZF07QcALCCQonKD1BYQ1S8AlKT5DUeqwLJ2g4AeEEpCeAnKBhLzN930/yGzuy46S8agOx5JmvmT8z36Qi6+bmZvT29vZZKXXst8Pf2PG2Zwul1IqZF8w8H4/H97GDJB9OJpPhYDD4KRxv47DzLDO/EFGmtZ7JgxUA3Looigci2otZYubHvmiIaKSU+tBiJzPGTMNeCQA3T9P0SThfMvM0TdP5xcUFQrnTmkwme2maniqlzojoUzDCzOfGmFv8LgFYax9C2HHToihOsyxb7eS15SVrLW49xjYzr4qiOIAP8oR78u8tnXOj93CO2w8Gg6+w65y7g808z2dEBHIDRJkKstZeK6W+yYfvcXNrLUJcAlBK3WmtzzzJn72vR2PMMeV5Pg/5SZLkqE/OfW4PmXmPiBDOZZZlLxK4tIu0wplPN859xP9aa6oBwIOu21trYQR5BLGaa0FE08vLyzts5HkO4qGkEe6KdBKYc+4AKeBgqQuAtRaOq/LpADpzzp0j54gUzklOSTIqpU4kgN9aa9Tv2srz/IqIJmJjqZRCQwmVgogciv251vokZktyrgZA5km+2KgSbH3XWoO4teXTA1ChAbWdq0jfC4AsnTbnAYkEK2tdIt0qAr6W/3gDS631sIuknuVV+Unyhfe2AuDDii6J9UNrfdUDAPjwy59bS8O/AIjmtIULJWhmvjfG1Er2vwKIcWYrALJ1KqUWWuujHimQQ6c2ej1HtqsCay3GcajxL01BIQH5Fv0cxjo6XbNFbxUBGG+01BURAcS8GQlfMch9aGblAIpwZLsIeBDVGPXkumVmPJOdEL0/qKlX59wwNta7IrAyxuzHcuxDC4eVqungwmuSJMdtUzUGoMpxj2mI8KEXRPUe5r5z7qpL0DTHf20cO+f2N6mhoPOIaMjMIyKC7IYmmDUJ1zKMoL5KnpR6oKHVqrm9qdx22W+09nL6AoBsna1jdBeHGyqgbO1BFVcyCXLcGJO9h0NpoznWQ48IAGQU8B5UTdYnp5uA+rBDTUkNUQ22SgNGVA+GCT6nXvB9J2p+k8+wD/2IvtBUWTXlVROhvuuhl7eVWV/nbefWRvqaCvZlhhZ62rPxbAJV6kfn3DSW0k4ZDss+h1Gx2uU5SZJVn2+Mv8yWa6D/Sa/gAAAAAElFTkSuQmCC"" width=""17"" alt=""Instagram"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0; margin-right: 12px;""></a>
      &bull;
      <a href=""https://www.google.com"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238; text-decoration: none;""><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABEAAAARCAMAAAAMs7fIAAAAIGNIUk0AAHomAACAhAAA+gAAAIDoAAB1MAAA6mAAADqYAAAXcJy6UTwAAAKmUExURes7J+8+Mu0+MO1DN/9XZepDNetDNgAAAO5ANO1ANO5wVes8NfBPQupDNvFgKfFGNudBNfRGQ+9ENuI/M+hBNOxDNfU/KVZ+3lh93ohqpppll4Rtr0aD8UOF9kOF9UKF9e9SLwBcAEOG9/mwCuRCOUSJ+/6/BOm4DDmkWEysSVAAQkOH9zSqUzKrRUCL6UKJ6j+PyT2WsECpW2KvQkaL/i6hVjPGUUGE9DSlVjOoVFa7XDqWpUGF9SedTjKpUzCmUTatVkqpXTOkYDKkX0GC9+pCNO1ENuxENetDNepDNepDNexDNe1DNe1DNOxDNO1ENexENuxDNe1DNe1ENelDNe5DNe1DNe1ENupDNutANu1ENexENexDNetDNetDNexENexDNe1ENuxENfORFO5SL+tDNexDNetDNetDM+NBKOtCNe1DNexDNfu8BfiiD+xDNupCNfy+Bfy/BPN6H+1LMkOG90SG90OFkSF9kOG9kKF9f2+Bf2+BfmwCkOI+USI+UOG90OG90OH9/2+Bf2/Bf/ABEOI+UOH90KG9/u/Bum3CkOI+ESI+UOG9kKF9UOG9/2+Bf6+BI6yMEKqTkOG9EOG90KF9kKF9kKF9UOG9kOG9vy8BdC4FmmtPjSqVDWoU0KG90KG9kKG97eyHEysSjSpUzSpUzSpUzKoUzSlQjGsPDuWqkKH80OG90GD9C6pVjWqVDSqVDSpUzSpUzSpUzSqUzWmX0OG90OG+DSqVDSqVDmcj0GJ5UN+/zSqUzSqVDSqVDSpVDSqUzOrUjGkVzOnUjSqVDSqVDSpUzSoUzSoUzSpUzWqUzSpU+tDNepDNetCNe5kKPmxCfu8Bfu9BUKF9Py9Bei6DEKG9TSpUzOpVDSoU0KF9TuWpkGH6zSoUjSoVf///2EEguoAAADOdFJOUwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABNdreT88cR7KD208/vSZwlQ3rgWNtvfpomWyvrCLQym+6cvBgIaYS1H664VjP7qPg4pKiorG7i/EEzb4uOgyK4IVvrIwBBS7fT7u4z+6T4YRUdFWNyWR+v+rhVb9FoMpvunLwYBEVzdwRg229+miZO+8/FWUN7zfAY9tPP70mMIE12t5PzxxHwpOh/ncwAAAAFiS0dE4V8Iz6YAAAAHdElNRQfpBwcMIzREb37zAAABNElEQVQY02NgYGRidnVz9/D08vbxZWFlY2fg4OTy8w84dx4IAoOCQ1jZGbh5QsOA/HPnQER4BC87A19kFJAZHRMbF5+QmMQvwM6QnHLhfGpaeoZgZlZ2jpCwCDtDbt7Fc/kFomLiEpJS0jKycvIMhUWXiksUFJVKy8qBoKKyiqH68pWaWmUV1br6BhBobGJovny5pVWNXbWt/SoYdADVXOvsUtdQ7e7p7e3tu3q1n2HCxOuTJmtqaU+ZOm36jJlXb8ximD1n7s1583V09fQNDBcsvLpwEcPiJbduL122fIXRylWr19y5s3Ydg/H6Dbdv39y4afOWrdvu3tu+w4TB1Gznrpu3gYI3b99/sHvPXnMGC0urffsPgMRuHzx0+Ii1DYOtnb3D0WPHT5w8dfrMWUcnZxcAFSWN0z+9o/IAAABEZVhJZk1NACoAAAAIAAGHaQAEAAAAAQAAABoAAAAAAAOgAQADAAAAAQABAACgAgAEAAAAAQAAACagAwAEAAAAAQAAACYAAAAAhPIgWwAAACV0RVh0ZGF0ZTpjcmVhdGUAMjAyNS0wNy0wN1QwMjo0NDo1MyswMDowMFC0kEYAAAAldEVYdGRhdGU6bW9kaWZ5ADIwMjUtMDctMDdUMDI6NDQ6NTMrMDA6MDAh6Sj6AAAAKHRFWHRkYXRlOnRpbWVzdGFtcAAyMDI1LTA3LTA3VDEyOjM1OjUyKzAwOjAwi32dWgAAABF0RVh0ZXhpZjpDb2xvclNwYWNlADEPmwJJAAAAEnRFWHRleGlmOkV4aWZPZmZzZXQAMjZTG6JlAAAAF3RFWHRleGlmOlBpeGVsWERpbWVuc2lvbgAzOGh5jgkAAAAXdEVYdGV4aWY6UGl4ZWxZRGltZW5zaW9uADM4te9XjAAAAABJRU5ErkJggg=="" width=""17"" alt=""Google"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0; margin-right: 12px;""></a>
    </p> -->
    <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
      Use of our service and website is subject to our
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Terms of Use</a> and
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Privacy Policy</a>.
    </p>
  </td>
</tr>
<tr>
  <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 16px;""></td>
</tr>
                  </table>
                </td>
              </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""vi"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <!--[if mso]>
    <xml><o:officedocumentsettings><o:pixelsperinch>96</o:pixelsperinch></o:officedocumentsettings></xml>
  <![endif]-->
    <title>Đặt lại mật khẩu của bạn</title>
    <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
    <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Yêu cầu đặt lại mật khẩu đã được nhận từ tài khoản EDISA của bạn</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Đặt lại mật khẩu của bạn"" lang=""vi"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
  <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
    <!-- <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
      <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;"">
    </a> -->
  </td>
</tr>
              <tr>
                <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                    <tr>
                      <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">Xin chào</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                          Yêu cầu đặt lại mật khẩu đã được nhận từ tài khoản
                          <span style=""font-weight: 600;"">EDISA</span> của bạn -
                          <a href=""mailto:{{Email}}"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">{{Email}}</a>
                          (ID: {{UserId}}) từ địa chỉ IP - <span style=""font-weight: 600;"">{{IpAddress}}</span> .
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">Nhấp vào nút bên dưới để đặt lại mật khẩu và đăng nhập.</p>
                        <!-- <a href=""{{ResetLink}}"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 24px; display: block; font-size: 16px; line-height: 100%; color: #7367f0; text-decoration: none;"">{{ResetLink}}</a> -->
                        <table cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <tr>
                            <td style=""mso-line-height-rule: exactly; mso-padding-alt: 16px 24px; border-radius: 4px; background-color: #7367f0; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                              <a href=""{{ResetLink}}"" style=""background-color: #7367f0; border-color: #7367f0; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: 600; font-size: 16px;"">Đặt lại mật khẩu &rarr;</a>
                            </td>
                          </tr>
                        </table>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-top: 24px; margin-bottom: 24px;"">
                          <span style=""font-weight: 600;"">Lưu ý:</span> Liên kết này có hiệu lực trong {{ExpiryMinutes}} phút kể từ thời điểm được
                          gửi đến bạn và chỉ có thể sử dụng để thay đổi mật khẩu một lần.
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0;"">
                          Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng
                          liên hệ với chúng tôi tại
                          <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>
                        </p>
                        <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
  <tr>
    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
      <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
    </td>
  </tr>
</table>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
  Không rõ vì sao bạn nhận được email này? Vui lòng
  <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">cho chúng tôi biết</a>.
</p>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">Cảm ơn, <br>Đội ngũ EDISA</p>
                      </td>
                    </tr>
                    <tr>
  <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
</tr>
<tr>
  <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
    <!-- <p align=""center"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 16px; cursor: default;"">
      ...
    </p> -->
    <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
      Việc sử dụng dịch vụ và website của chúng tôi tuân theo
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Điều khoản sử dụng</a> và
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Chính sách bảo mật</a>.
    </p>
  </td>
</tr>
                  </table>
                </td>
              </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""ja"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <!--[if mso]>
    <xml><o:officedocumentsettings><o:pixelsperinch>96</o:pixelsperinch></o:officedocumentsettings></xml>
  <![endif]-->
    <title>パスワードのリセット</title>
    <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
    <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">EDISAアカウントからパスワードリセットのリクエストが受信されました</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""パスワードのリセット"" lang=""ja"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
  <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
    <!-- <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
      <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;"">
    </a> -->
  </td>
</tr>
              <tr>
                <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                    <tr>
                      <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">こんにちは</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}様！</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                          あなたの
                          <span style=""font-weight: 600;"">EDISA</span>アカウントからパスワードリセットのリクエストが受信されました -
                          <a href=""mailto:{{Email}}"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">{{Email}}</a>
                          (ID: {{UserId}}) IPアドレス - <span style=""font-weight: 600;"">{{IpAddress}}</span> から。
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">下のボタンをクリックしてパスワードをリセットし、ログインしてください。</p>
                        <!-- <a href=""{{ResetLink}}"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 24px; display: block; font-size: 16px; line-height: 100%; color: #7367f0; text-decoration: none;"">{{ResetLink}}</a> -->
                        <table cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <tr>
                            <td style=""mso-line-height-rule: exactly; mso-padding-alt: 16px 24px; border-radius: 4px; background-color: #7367f0; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                              <a href=""{{ResetLink}}"" style=""background-color: #7367f0; border-color: #7367f0; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: 600; font-size: 16px;"">パスワードリセット &rarr;</a>
                            </td>
                          </tr>
                        </table>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-top: 24px; margin-bottom: 24px;"">
                          <span style=""font-weight: 600;"">注意:</span> このリンクは送信された時点から{{ExpiryMinutes}}分間有効で、
                          パスワードの変更に一度だけ使用できます。
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0;"">
                          パスワードリセットをリクエストしていない場合は、
                          <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>
                          までお問い合わせください。
                        </p>
                        <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
  <tr>
    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
      <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
    </td>
  </tr>
</table>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
  このメールを受け取った理由がわからない場合は、
  <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">お知らせください</a>。
</p>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">ありがとうございます、<br>EDISAチーム</p>
                      </td>
                    </tr>
                    <tr>
  <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
</tr>
<tr>
  <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
    <!-- <p align=""center"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 16px; cursor: default;"">
      ...
    </p> -->
    <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
      当社のサービスとウェブサイトのご利用は、
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">利用規約</a>と
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">プライバシーポリシー</a>に従います。
    </p>
  </td>
</tr>
                  </table>
                </td>
              </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""en"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>Password Changed Successfully</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
  <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Your password has been changed successfully</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Password Changed Successfully"" lang=""en"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
              <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <!-- <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;""> -->
                </a>
              </td>
            </tr>
            <tr>
              <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                  <tr>
                    <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">Hey</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                        Your password has been changed successfully.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        If you did not perform this action, please contact us immediately at
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>.
                      </p>
                      <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                        <tr>
                          <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
                            <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
                          </td>
                        </tr>
                      </table>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
                        Not sure why you received this email? Please
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">let us know</a>.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">Thanks, <br>The EDISA Team</p>
                    </td>
                  </tr>
                  <tr>
                    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
                  </tr>
                  <tr>
                    <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
                      <!-- <p align=""center"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 16px; cursor: default;"">
                        <a href=""https://www.facebook.com/infjz"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238; text-decoration: none;""><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAABHNCSVQICAgIfAhkiAAAAr9JREFUWEfFmF9y2jAQxndt89zcoOQEJScoOUFyg6bPjAw+QcsJTKzhOdyg5ASlJ0g4QcMJGt69bGc9MmOM/0g2ne4rsvTTp0+7KxAcIo7jq8FgcHc4HMaIOASAKwAYmSleAeCdmd+YeT2dTp8dpga0Gay1HgPAFADubcbLGGZ+B4A1Ec2jKHpr+64RJI7jYRAEsQtA1YLMvCKiKIoigauMWpAkSR4AIEZEkb93iEK+799OJhM5wrOoBNFafwOA771Xr5iAmb+GYbgq/3QGorUWAAH5Z1EFcwIix4GITx0J9sz8iohya+RGZYGIn8vzVR3TEUSM6fv+SwdP7ADgQSm1qdpAncICQ0TXuYGPIFrrHx1uxzZN03HTbWg56rlSKvNiBmLyxE/XI0nTVHbUmCOaQIwqNzJHBpIkyRoR71xAmPk5DMPWBGdh/kel1AwlbQdB8McFwow9ylr8VmstcFNmHtn4TUpCGIbX2OOm3JYNajLxb9dNeZ53g1prSS5fXD8GgDOQJElmiCglwTXmosim6q5bzHQGYuGHumkfRZGXQim3WP845GIgzPxLQNhl9cLY/wOilLLqXcqbslE8V0TK8qc2VXqAtCqegdiatQuIw3XOzLowbWCjKF1AbEsHM0cCIplQCl5jMLP0rccgom252JkK/rEwTNoK6fQaQ2pWnxR/qVuzU0oN8+rbJbteCiSrWRmIg6mKEl8CZJ+m6VCOuNgYuapyCZDTxsioIu2A5JSi2ZpM1hdkq5TKX4mnLz3b62bo+oDIkYyK3d1Z2nboT7qC7D3PG5cfWpX1wxKmC0glhChcW8jMMYmB6zzjCiId/31ds932CBcDS7sv/wSUwxZkDwCL/NlQ536r0m7yjKTqGQB8sDTrjpkXRLRqevfkYFYgxV0sl8sREY2JaF2WWeqWPDd939/UvfrrFPkLB5ilt2wxKssAAAAASUVORK5CYII="" width=""17"" alt=""Facebook"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0; margin-right: 12px;""></a>
                        &bull;
                        <a href=""https://www.twitter.com/infjz"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238; text-decoration: none;""><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACMAAAAdCAYAAAAgqdWEAAAABHNCSVQICAgIfAhkiAAAAptJREFUSEu1V91x2kAQ3r3Te9xBSAWhAzsdkAoCz8zKVgUhFchIwzsdBFdgUkHsCmwqiHmFky6zmjtGPyehH7gZXrjb3W+//RXChU8Yhjee5/0AgAkA3LF6rfU7AGyllMv5fP5SNrlarcb8P9qLOI4nRLQZgo2VJknyjIg3dXq01o++7wcMWghxK4R4AIA/RLTIwBhv/gHAkoj4svNhIGma/m0pyOyMzdudUmocBMFHBoZZAYDfhtK17/uzlkqzZ+yMlPKtiZEafXshxN3hcPjwPG9kwSwA4GdOYKOUmjHaNqCiKHpAxLDN29KbLQBwSL8opWrBZEmHiDMiYoHGE0XRFhFvz71rYueUwPkwOQSYpSAIAq4I54njWPcE8iqEmNoKyycwG/vUoJS9Xx+Px6dy+AaA+ZZnPl/ajwBw39JDDh2De0/TlJ1YI+KopWz+mRuMqSouua89lPYVqYLhamAQWuuNEGIJAJ/7au8o5wbTszQ72i4+J6JTmvCNTeCR53lvgzR3F34lItuFM+kTsoG9ojsUx+g5gTHz6Vx59zFaJ/O9PJgLMTPDjif3tRN4R0SVVlAAwy4Yhri6plcE9YtXhjJlFTBRFG0Q0XbibDm68NnzUHQN4QqYOI7LE/zCWMDJSqGarEUTJm731+jElXLOe1phxuaNlJLD1XctcLGZLVKuHdg+doKxl2a14LBdgqVKKZ9NYJdLYRiOpJS8aPeZzLyozXzfX59LvkZmTP7ca615razd+JuMtAVSm8D8CYGIXNbTviAAYCeEmDTlSCVM5ltnwb1Faz0eYDyve6mUWrRd6AsJfKGuuwcA3pcZRO2+3BRSV9Pjbyj+8Xg/V0UZAK31NkmSTVcmOlWTYaywc1gFSqmXocbLYP4DC8Yg48wi1iQAAAAASUVORK5CYII="" width=""17"" alt=""Twitter"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0; margin-right: 12px;""></a>
                        &bull;
                        <a href=""https://www.instagram.com/infjz"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238; text-decoration: none;""><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAABHNCSVQICAgIfAhkiAAAA2VJREFUWEe9V0FW2zAQ1djZF07QcALCCQonKD1BYQ1S8AlKT5DUeqwLJ2g4AeEEpCeAnKBhLzN930/yGzuy46S8agOx5JmvmT8z36Qi6+bmZvT29vZZKXXst8Pf2PG2Zwul1IqZF8w8H4/H97GDJB9OJpPhYDD4KRxv47DzLDO/EFGmtZ7JgxUA3Looigci2otZYubHvmiIaKSU+tBiJzPGTMNeCQA3T9P0SThfMvM0TdP5xcUFQrnTmkwme2maniqlzojoUzDCzOfGmFv8LgFYax9C2HHToihOsyxb7eS15SVrLW49xjYzr4qiOIAP8oR78u8tnXOj93CO2w8Gg6+w65y7g808z2dEBHIDRJkKstZeK6W+yYfvcXNrLUJcAlBK3WmtzzzJn72vR2PMMeV5Pg/5SZLkqE/OfW4PmXmPiBDOZZZlLxK4tIu0wplPN859xP9aa6oBwIOu21trYQR5BLGaa0FE08vLyzts5HkO4qGkEe6KdBKYc+4AKeBgqQuAtRaOq/LpADpzzp0j54gUzklOSTIqpU4kgN9aa9Tv2srz/IqIJmJjqZRCQwmVgogciv251vokZktyrgZA5km+2KgSbH3XWoO4teXTA1ChAbWdq0jfC4AsnTbnAYkEK2tdIt0qAr6W/3gDS631sIuknuVV+Unyhfe2AuDDii6J9UNrfdUDAPjwy59bS8O/AIjmtIULJWhmvjfG1Er2vwKIcWYrALJ1KqUWWuujHimQQ6c2ej1HtqsCay3GcajxL01BIQH5Fv0cxjo6XbNFbxUBGG+01BURAcS8GQlfMch9aGblAIpwZLsIeBDVGPXkumVmPJOdEL0/qKlX59wwNta7IrAyxuzHcuxDC4eVqungwmuSJMdtUzUGoMpxj2mI8KEXRPUe5r5z7qpL0DTHf20cO+f2N6mhoPOIaMjMIyKC7IYmmDUJ1zKMoL5KnpR6oKHVqrm9qdx22W+09nL6AoBsna1jdBeHGyqgbO1BFVcyCXLcGJO9h0NpoznWQ48IAGQU8B5UTdYnp5uA+rBDTUkNUQ22SgNGVA+GCT6nXvB9J2p+k8+wD/2IvtBUWTXlVROhvuuhl7eVWV/nbefWRvqaCvZlhhZ62rPxbAJV6kfn3DSW0k4ZDss+h1Gx2uU5SZJVn2+Mv8yWa6D/Sa/gAAAAAElFTkSuQmCC"" width=""17"" alt=""Instagram"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0; margin-right: 12px;""></a>
                        &bull;
                        <a href=""https://www.google.com"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238; text-decoration: none;""><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABEAAAARCAMAAAAMs7fIAAAAIGNIUk0AAHomAACAhAAA+gAAAIDoAAB1MAAA6mAAADqYAAAXcJy6UTwAAAKmUExURes7J+8+Mu0+MO1DN/9XZepDNetDNgAAAO5ANO1ANO5wVes8NfBPQupDNvFgKfFGNudBNfRGQ+9ENuI/M+hBNOxDNfU/KVZ+3lh93ohqpppll4Rtr0aD8UOF9kOF9UKF9e9SLwBcAEOG9/mwCuRCOUSJ+/6/BOm4DDmkWEysSVAAQkOH9zSqUzKrRUCL6UKJ6j+PyT2WsECpW2KvQkaL/i6hVjPGUUGE9DSlVjOoVFa7XDqWpUGF9SedTjKpUzCmUTatVkqpXTOkYDKkX0GC9+pCNO1ENuxENetDNepDNepDNexDNe1DNe1DNOxDNO1ENexENuxDNe1DNe1ENelDNe5DNe1DNe1ENupDNutANu1ENexENexDNetDNetDNexENexDNe1ENuxENfORFO5SL+tDNexDNetDNetDM+NBKOtCNe1DNexDNfu8BfiiD+xDNupCNfy+Bfy/BPN6H+1LMkOG90SG90OFkSF9kOG9kKF9f2+Bf2+BfmwCkOI+USI+UOG90OG90OH9/2+Bf2/Bf/ABEOI+UOH90KG9/u/Bum3CkOI+ESI+UOG9kKF9UOG9/2+Bf6+BI6yMEKqTkOG9EOG90KF9kKF9kKF9UOG9kOG9vy8BdC4FmmtPjSqVDWoU0KG90KG9kKG97eyHEysSjSpUzSpUzSpUzKoUzSlQjGsPDuWqkKH80OG90GD9C6pVjWqVDSqVDSpUzSpUzSpUzSqUzWmX0OG90OG+DSqVDSqVDmcj0GJ5UN+/zSqUzSqVDSqVDSpVDSqUzOrUjGkVzOnUjSqVDSqVDSpUzSoUzSoUzSpUzWqUzSpU+tDNepDNetCNe5kKPmxCfu8Bfu9BUKF9Py9Bei6DEKG9TSpUzOpVDSoU0KF9TuWpkGH6zSoUjSoVf///2EEguoAAADOdFJOUwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABNdreT88cR7KD208/vSZwlQ3rgWNtvfpomWyvrCLQym+6cvBgIaYS1H664VjP7qPg4pKiorG7i/EEzb4uOgyK4IVvrIwBBS7fT7u4z+6T4YRUdFWNyWR+v+rhVb9FoMpvunLwYBEVzdwRg229+miZO+8/FWUN7zfAY9tPP70mMIE12t5PzxxHwpOh/ncwAAAAFiS0dE4V8Iz6YAAAAHdElNRQfpBwcMIzREb37zAAABNElEQVQY02NgYGRidnVz9/D08vbxZWFlY2fg4OTy8w84dx4IAoOCQ1jZGbh5QsOA/HPnQER4BC87A19kFJAZHRMbF5+QmMQvwM6QnHLhfGpaeoZgZlZ2jpCwCDtDbt7Fc/kFomLiEpJS0jKycvIMhUWXiksUFJVKy8qBoKKyiqH68pWaWmUV1br6BhBobGJovny5pVWNXbWt/SoYdADVXOvsUtdQ7e7p7e3tu3q1n2HCxOuTJmtqaU+ZOm36jJlXb8ximD1n7s1583V09fQNDBcsvLpwEcPiJbduL122fIXRylWr19y5s3Ydg/H6Dbdv39y4afOWrdvu3tu+w4TB1Gznrpu3gYI3b99/sHvPXnMGC0urffsPgMRuHzx0+Ii1DYOtnb3D0WPHT5w8dfrMWUcnZxcAFSWN0z+9o/IAAABEZVhJZk1NACoAAAAIAAGHaQAEAAAAAQAAABoAAAAAAAOgAQADAAAAAQABAACgAgAEAAAAAQAAACagAwAEAAAAAQAAACYAAAAAhPIgWwAAACV0RVh0ZGF0ZTpjcmVhdGUAMjAyNS0wNy0wN1QwMjo0NDo1MyswMDowMFC0kEYAAAAldEVYdGRhdGU6bW9kaWZ5ADIwMjUtMDctMDdUMDI6NDQ6NTMrMDA6MDAh6Sj6AAAAKHRFWHRkYXRlOnRpbWVzdGFtcAAyMDI1LTA3LTA3VDEyOjM1OjUyKzAwOjAwi32dWgAAABF0RVh0ZXhpZjpDb2xvclNwYWNlADEPmwJJAAAAEnRFWHRleGlmOkV4aWZPZmZzZXQAMjZTG6JlAAAAF3RFWHRleGlmOlBpeGVsWERpbWVuc2lvbgAzOGh5jgkAAAAXdEVYdGV4aWY6UGl4ZWxZRGltZW5zaW9uADM4te9XjAAAAABJRU5ErkJggg=="" width=""17"" alt=""Google"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0; margin-right: 12px;""></a>
                      </p> -->
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
                        Use of our service and website is subject to our
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Terms of Use</a> and
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Privacy Policy</a>.
                      </p>
                    </td>
                  </tr>
                  <tr>
                    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 16px;""></td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""vi"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>Mật khẩu đã được thay đổi thành công</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
  <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Mật khẩu của bạn đã được thay đổi thành công</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Mật khẩu đã được thay đổi thành công"" lang=""vi"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
              <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <!-- <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;""> -->
                </a>
              </td>
            </tr>
            <tr>
              <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                  <tr>
                    <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">Xin chào</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                        Mật khẩu của bạn đã được thay đổi thành công.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ với chúng tôi ngay lập tức tại
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>.
                      </p>
                      <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                        <tr>
                          <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
                            <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
                          </td>
                        </tr>
                      </table>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
                        Không rõ vì sao bạn nhận được email này? Vui lòng
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">cho chúng tôi biết</a>.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">Cảm ơn, <br>Đội ngũ EDISA</p>
                    </td>
                  </tr>
                  <tr>
                    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
                  </tr>
                  <tr>
                    <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
                      <!-- <p align=""center"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 16px; cursor: default;"">
                        ...
                      </p> -->
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
                        Việc sử dụng dịch vụ và website của chúng tôi tuân theo
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Điều khoản sử dụng</a> và
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Chính sách bảo mật</a>.
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""ja"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>パスワードが正常に変更されました</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
  <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">パスワードが正常に変更されました</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""パスワードが正常に変更されました"" lang=""ja"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
              <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <!-- <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;""> -->
                </a>
              </td>
            </tr>
            <tr>
              <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                  <tr>
                    <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">こんにちは</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}様！</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                        パスワードが正常に変更されました。
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        この変更を行っていない場合は、すぐに
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>
                        までお問い合わせください。
                      </p>
                      <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                        <tr>
                          <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
                            <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
                          </td>
                        </tr>
                      </table>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
                        このメールを受け取った理由がわからない場合は、
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">お知らせください</a>。
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">ありがとうございます、<br>EDISAチーム</p>
                    </td>
                  </tr>
                  <tr>
                    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
                  </tr>
                  <tr>
                    <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
                      <!-- <p align=""center"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 16px; cursor: default;"">
                        ...
                      </p> -->
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
                        当社のサービスとウェブサイトのご利用は、
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">利用規約</a>と
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">プライバシーポリシー</a>に従います。
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""en"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>Account Deactivated</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
  <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Your account has been deactivated</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Account Deactivated"" lang=""en"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
              <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <!-- <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;""> -->
                </a>
              </td>
            </tr>
            <tr>
              <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                  <tr>
                    <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">Hello</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color: #ff5850;"">{{Username}}!</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                        Your account has been deactivated.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        If you have any questions or believe this was done in error, please contact us at
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>.
                      </p>
                      <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                        <tr>
                          <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
                            <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
                          </td>
                        </tr>
                      </table>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
                        Not sure why you received this email? Please
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">let us know</a>.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">Best regards, <br>The EDISA Team</p>
                    </td>
                  </tr>
                  <tr>
                    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
                  </tr>
                  <tr>
                    <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
                      <!-- <p align=""center"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 16px; cursor: default;"">
                        ...
                      </p> -->
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
                        Use of our service and website is subject to our
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Terms of Use</a> and
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Privacy Policy</a>.
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""vi"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>Tài khoản đã bị vô hiệu hóa</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
  <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Tài khoản của bạn đã bị vô hiệu hóa</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Tài khoản đã bị vô hiệu hóa"" lang=""vi"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
              <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <!-- <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;""> -->
                </a>
              </td>
            </tr>
            <tr>
              <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                  <tr>
                    <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">Xin chào</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color: #ff5850;"">{{Username}}!</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                        Tài khoản của bạn đã bị vô hiệu hóa.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        Nếu bạn có bất kỳ câu hỏi nào hoặc tin rằng điều này được thực hiện do nhầm lẫn, vui lòng liên hệ với chúng tôi tại
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>.
                      </p>
                      <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                        <tr>
                          <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
                            <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
                          </td>
                        </tr>
                      </table>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
                        Không rõ vì sao bạn nhận được email này? Vui lòng
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">cho chúng tôi biết</a>.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">Trân trọng, <br>Đội ngũ EDISA</p>
                    </td>
                  </tr>
                  <tr>
                    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
                  </tr>
                  <tr>
                    <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
                      <!-- <p align=""center"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 16px; cursor: default;"">
                        ...
                      </p> -->
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
                        Việc sử dụng dịch vụ và website của chúng tôi tuân theo
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Điều khoản sử dụng</a> và
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Chính sách bảo mật</a>.
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""ja"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>アカウントが無効化されました</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
  <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">アカウントが無効化されました</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""アカウントが無効化されました"" lang=""ja"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
              <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <!-- <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;""> -->
                </a>
              </td>
            </tr>
            <tr>
              <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                  <tr>
                    <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">こんにちは</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color: #ff5850;"">{{Username}}様！</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                        アカウントが無効化されました。
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        ご不明な点がございましたら、またはこれが誤って行われたと思われる場合は、
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>
                        までお問い合わせください。
                      </p>
                      <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                        <tr>
                          <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
                            <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
                          </td>
                        </tr>
                      </table>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
                        このメールを受け取った理由がわからない場合は、
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">お知らせください</a>。
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">よろしくお願いいたします、<br>EDISAチーム</p>
                    </td>
                  </tr>
                  <tr>
                    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
                  </tr>
                  <tr>
                    <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
                      <!-- <p align=""center"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 16px; cursor: default;"">
                        ...
                      </p> -->
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
                        当社のサービスとウェブサイトのご利用は、
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">利用規約</a>と
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">プライバシーポリシー</a>に従います。
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <title>Welcome to EDISA</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
  <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Welcome to EDISA - Google Login</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Welcome Google User"" lang=""en"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
              <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <!-- <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;""> -->
                </a>
              </td>
            </tr>
            <tr>
              <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                  <tr>
                    <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">Hello</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                        Welcome to EDISA! 🎉
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        You have successfully signed in using your Google account. Your email has been automatically verified, and you can now start exploring all features of our platform.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        <strong>Important:</strong> To enhance your account security, we recommend setting up a password for your account. This will allow you to log in directly to our system without using Google authentication.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 32px; text-align: center;"">
                        <a href=""{{ResetLink}}"" style=""background-color: #7367f0; border-color: #7367f0; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: 600; font-size: 16px;"">
                          Set Up Your Password
                        </a>
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px; font-size: 14px; color: #888;"">
                        <strong>Note:</strong> This link will expire in <strong>1 hour</strong> for security reasons. If you don't set up a password now, you can always use Google authentication to log in.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        If you have any questions or need support, feel free to contact us at
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>.
                      </p>
                      <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                        <tr>
                          <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
                            <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
                          </td>
                        </tr>
                      </table>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
                        Not sure why you received this email? Please
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">let us know</a>.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">Thanks, <br>The EDISA Team</p>
                    </td>
                  </tr>
                  <tr>
                    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
                  </tr>
                  <tr>
                    <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
                      <!-- social links commented -->
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
                        Use of our service and website is subject to our
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Terms of Use</a> and
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Privacy Policy</a>.
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""vi"">
<head>
  <meta charset=""utf-8"">
  <title>Chào mừng đến với EDISA</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
  <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Chào mừng đến EDISA - Đăng nhập Google</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Chào mừng người dùng Google"" lang=""vi"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
              <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <!-- <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;""> -->
                </a>
              </td>
            </tr>
            <tr>
              <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                  <tr>
                    <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">Xin chào</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                        Chào mừng đến với EDISA! 🎉
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        Bạn đã đăng nhập thành công bằng tài khoản Google. Email của bạn đã được xác minh tự động và bạn có thể bắt đầu khám phá tất cả tính năng của nền tảng.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        <strong>Quan trọng:</strong> Để tăng cường bảo mật cho tài khoản, chúng tôi khuyến nghị bạn thiết lập mật khẩu. Việc này giúp bạn có thể đăng nhập trực tiếp mà không cần dùng xác thực Google.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 32px; text-align: center;"">
                        <a href=""{{ResetLink}}"" style=""background-color: #7367f0; border-color: #7367f0; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: 600; font-size: 16px;"">
                          Thiết lập mật khẩu
                        </a>
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px; font-size: 14px; color: #888;"">
                        <strong>Lưu ý:</strong> Liên kết này sẽ hết hạn sau <strong>1 giờ</strong> vì lý do bảo mật. Nếu chưa thiết lập ngay, bạn vẫn có thể đăng nhập bằng xác thực Google.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        Nếu bạn có bất kỳ câu hỏi hoặc cần hỗ trợ, vui lòng liên hệ
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>.
                      </p>
                      <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                        <tr>
                          <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
                            <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
                          </td>
                        </tr>
                      </table>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
                        Không rõ vì sao bạn nhận được email này? Vui lòng
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">cho chúng tôi biết</a>.
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">Cảm ơn, <br>Đội ngũ EDISA</p>
                    </td>
                  </tr>
                  <tr>
                    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
                  </tr>
                  <tr>
                    <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
                        Việc sử dụng dịch vụ và website của chúng tôi tuân theo
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Điều khoản sử dụng</a> và
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Chính sách bảo mật</a>.
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""ja"">
<head>
  <meta charset=""utf-8"">
  <title>EDISAへようこそ</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
  <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">EDISAへようこそ - Googleログイン</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Googleユーザーへようこそ"" lang=""ja"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
              <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
                <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <!-- <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;""> -->
                </a>
              </td>
            </tr>
            <tr>
              <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                  <tr>
                    <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">こんにちは</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}様！</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                        EDISAへようこそ！🎉
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        Googleアカウントで正常にサインインしました。メールは自動的に確認され、プラットフォームのすべての機能をご利用いただけます。
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        <strong>重要:</strong> アカウントのセキュリティ強化のため、パスワードの設定をおすすめします。これにより、Google認証を使わずに直接ログインできます。
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 32px; text-align: center;"">
                        <a href=""{{ResetLink}}"" style=""background-color: #7367f0; border-color: #7367f0; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: 600; font-size: 16px;"">
                          パスワードを設定する
                        </a>
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px; font-size: 14px; color: #888;"">
                        <strong>注意:</strong> セキュリティのため、このリンクは<strong>1時間</strong>で有効期限が切れます。今すぐ設定しない場合でも、Google認証でログインできます。
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 24px;"">
                        ご不明な点やサポートが必要な場合は、
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>
                        までお気軽にお問い合わせください。
                      </p>
                      <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                        <tr>
                          <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
                            <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
                          </td>
                        </tr>
                      </table>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
                        このメールを受け取った理由がわからない場合は、
                        <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">お知らせください</a>。
                      </p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">ありがとうございます、<br>EDISAチーム</p>
                    </td>
                  </tr>
                  <tr>
                    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
                  </tr>
                  <tr>
                    <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
                        当社のサービスとウェブサイトのご利用は、
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">利用規約</a>と
                        <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">プライバシーポリシー</a>に従います。
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""en"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <!--[if mso]>
    <xml><o:officedocumentsettings><o:pixelsperinch>96</o:pixelsperinch></o:officedocumentsettings></xml>
  <![endif]-->
    <title>Account Restored 🎉</title>
    <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
    <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Your account on EDISA has been restored.</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Account Restored 🎉"" lang=""en"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
  <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
    <!-- <a href=""#"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
      <img src=""https://via.placeholder.com/155x50/667eea/ffffff?text=Microservice+System"" width=""155"" alt=""Microservice System"" style=""max-width: 100%; vertical-align: middle; line-height: 100%; border: 0;"">
    </a> -->
  </td>
</tr>
              <tr>
                <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                    <tr>
                      <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">Hey</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color: #28a745;"">{{Username}}!</p>
                        <p class=""sm-leading-32"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                          Your account has been restored! 🎉
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">Great news! Your account has been successfully restored and is now active again.</p>
                        <div style=""background-color: #d4edda; border: 1px solid #c3e6cb; color: #155724; padding: 15px; border-radius: 5px; margin: 15px 0;"">
                          <strong>Account Details:</strong><br>
                          • Username: {{Username}}<br>
                          • Restored At: {{RestoredAt}}<br>
                          • Reason: {{Reason}}
                        </div>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">You can now log in to your account and access all features normally.</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 16px 0; text-align: center;"">
                          <a href=""{{LoginUrl}}"" style=""background-color: #7367f0; color: #fff; padding: 10px 24px; border-radius: 6px; text-decoration: none; font-weight: 600; display: inline-block;"">Log in to your account</a>
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0;"">
                          If you have any questions or need assistance, please don't hesitate to contact our support team at
                          <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>
                        </p>
                        <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
  <tr>
    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
      <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
    </td>
  </tr>
</table>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
  Thank you for your patience and understanding.
</p>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">Best regards, <br>The EDISA Team</p>
                      </td>
                    </tr>
                    <tr>
  <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
</tr>
<tr>
  <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
    <!-- social links commented -->
    <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
      Use of our service and website is subject to our
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Terms of Use</a> and
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Privacy Policy</a>.
    </p>
  </td>
</tr> 
<tr>
  <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 16px;""></td>
</tr>
                  </table>
                </td>
              </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""vi"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>Khôi phục tài khoản 🎉</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
    <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Tài khoản của bạn trên EDISA đã được khôi phục.</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Khôi phục tài khoản 🎉"" lang=""vi"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
  <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;""></td>
</tr>
              <tr>
                <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                    <tr>
                      <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">Xin chào</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color: #28a745;"">{{Username}}!</p>
                        <p class=""sm-leading-32"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                          Tài khoản của bạn đã được khôi phục! 🎉
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">Tin vui! Tài khoản của bạn đã được khôi phục thành công và hiện đã hoạt động trở lại.</p>
                        <div style=""background-color: #d4edda; border: 1px solid #c3e6cb; color: #155724; padding: 15px; border-radius: 5px; margin: 15px 0;"">
                          <strong>Chi tiết tài khoản:</strong><br>
                          • Tên người dùng: {{Username}}<br>
                          • Thời gian khôi phục: {{RestoredAt}}<br>
                          • Lý do: {{Reason}}
                        </div>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">Bây giờ bạn có thể đăng nhập và sử dụng đầy đủ các tính năng như bình thường.</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 16px 0; text-align: center;"">
                          <a href=""{{LoginUrl}}"" style=""background-color: #7367f0; color: #fff; padding: 10px 24px; border-radius: 6px; text-decoration: none; font-weight: 600; display: inline-block;"">Đăng nhập vào tài khoản của bạn</a>
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0;"">
                          Nếu bạn có bất kỳ câu hỏi hoặc cần hỗ trợ, vui lòng liên hệ đội ngũ hỗ trợ qua
                          <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>
                        </p>
                        <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
  <tr>
    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
      <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
    </td>
  </tr>
</table>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
  Cảm ơn bạn vì sự kiên nhẫn và thấu hiểu.
</p>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">Trân trọng, <br>Đội ngũ EDISA</p>
                      </td>
                    </tr>
                    <tr>
  <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
</tr>
<tr>
  <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
    <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
      Việc sử dụng dịch vụ và website của chúng tôi tuân theo
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Điều khoản sử dụng</a> và
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">Chính sách bảo mật</a>.
    </p>
  </td>
</tr>
                  </table>
                </td>
              </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""ja"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>アカウントが復元されました 🎉</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
    <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">EDISA のアカウントが復元されました。</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""アカウントが復元されました 🎉"" lang=""ja"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
  <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;""></td>
</tr>
              <tr>
                <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                  <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                    <tr>
                      <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-bottom: 0; font-size: 20px; font-weight: 600;"">こんにちは</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color: #28a745;"">{{Username}}様！</p>
                        <p class=""sm-leading-32"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px; font-size: 24px; font-weight: 600; color: #263238;"">
                          アカウントが復元されました！🎉
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">うれしいお知らせです。アカウントは正常に復元され、再び有効になりました。</p>
                        <div style=""background-color: #d4edda; border: 1px solid #c3e6cb; color: #155724; padding: 15px; border-radius: 5px; margin: 15px 0;"">
                          <strong>アカウント情報:</strong><br>
                          • ユーザー名: {{Username}}<br>
                          • 復元日時: {{RestoredAt}}<br>
                          • 理由: {{Reason}}
                        </div>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">今すぐログインして、通常どおりすべての機能をご利用いただけます。</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 16px 0; text-align: center;"">
                          <a href=""{{LoginUrl}}"" style=""background-color: #7367f0; color: #fff; padding: 10px 24px; border-radius: 6px; text-decoration: none; font-weight: 600; display: inline-block;"">アカウントにログイン</a>
                        </p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0;"">
                          ご不明な点やサポートが必要な場合は、
                          <a href=""mailto:support@edisa.com"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">support@edisa.com</a>
                          までお気軽にお問い合わせください。
                        </p>
                        <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
  <tr>
    <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; padding-top: 32px; padding-bottom: 32px;"">
      <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div>
    </td>
  </tr>
</table>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">
  ご理解とご協力に感謝いたします。
</p>
<p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin: 0; margin-bottom: 16px;"">よろしくお願いいたします、<br>EDISAチーム</p>
                      </td>
                    </tr>
                    <tr>
  <td style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; height: 20px;""></td>
</tr>
<tr>
  <td style=""mso-line-height-rule: exactly; padding-left: 48px; padding-right: 48px; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #eceff1;"">
    <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #263238;"">
      当社のサービスとウェブサイトのご利用は、
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">利用規約</a>と
      <a href=""#"" class=""hover-underline"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; color: #7367f0; text-decoration: none;"">プライバシーポリシー</a>に従います。
    </p>
  </td>
</tr>
                  </table>
                </td>
              </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""en"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>File Uploaded Successfully</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
  <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Your file was uploaded successfully</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""File Uploaded Successfully"" lang=""en"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""mso-line-height-rule: exactly; background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table class=""sm-w-full"" style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
              <td class=""sm-py-32 sm-px-24"" style=""mso-line-height-rule: exactly; padding: 48px; text-align: center; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;""></td>
            </tr>
            <tr>
              <td align=""center"" class=""sm-px-24"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
                <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                  <tr>
                    <td class=""sm-px-24"" style=""mso-line-height-rule: exactly; border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif; font-size: 16px; line-height: 24px; color: #626262;"">
                      <p style=""margin: 0; font-size: 20px; font-weight: 600;"">Hello</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                      <p style=""margin: 16px 0 8px; font-size: 20px; font-weight: 600; color: #263238;"">Your file has been uploaded successfully.</p>
                      <div style=""background-color: #f8fafc; border: 1px solid #e2e8f0; color: #334155; padding: 14px 16px; border-radius: 6px; margin: 16px 0;"">
                        <strong>File Details</strong><br>
                        • File Name: {{FileName}}<br>
                        • File Size: {{FileSize}}<br>
                        • Upload Time: {{UploadTime}}<br>
                        • IP Address: {{IpAddress}}
                      </div>
                      <p style=""margin: 0;"">Thank you for using our service!</p>
                      <table style=""width: 100%;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                        <tr>
                          <td style=""padding-top: 32px; padding-bottom: 32px;""><div style=""height: 1px; background-color: #eceff1; line-height: 1px;"">&zwnj;</div></td>
                        </tr>
                      </table>
                      <p style=""margin: 0; color: #263238;"">Use of our service and website is subject to our <a href=""#"" style=""color: #7367f0; text-decoration: none;"">Terms of Use</a> and <a href=""#"" style=""color: #7367f0; text-decoration: none;"">Privacy Policy</a>.</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""vi"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>Tệp đã được tải lên thành công</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
  <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">Tệp của bạn đã được tải lên thành công</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Tệp đã được tải lên thành công"" lang=""vi"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr><td style=""padding: 48px; text-align: center;""></td></tr>
            <tr>
              <td style=""font-family: 'Montserrat', sans-serif;"" align=""center"">
                <table style=""width: 100%;"" role=""presentation"">
                  <tr>
                    <td style=""border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-size: 16px; line-height: 24px; color: #626262;"">
                      <p style=""margin: 0; font-size: 20px; font-weight: 600;"">Xin chào</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                      <p style=""margin: 16px 0 8px; font-size: 20px; font-weight: 600; color: #263238;"">Tệp của bạn đã được tải lên thành công.</p>
                      <div style=""background-color: #f8fafc; border: 1px solid #e2e8f0; color: #334155; padding: 14px 16px; border-radius: 6px; margin: 16px 0;"">
                        <strong>Chi tiết tệp</strong><br>
                        • Tên tệp: {{FileName}}<br>
                        • Kích thước: {{FileSize}}<br>
                        • Thời gian tải lên: {{UploadTime}}<br>
                        • Địa chỉ IP: {{IpAddress}}
                      </div>
                      <p style=""margin: 0;"">Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                      <table style=""width: 100%;"" role=""presentation""><tr><td style=""padding-top: 32px; padding-bottom: 32px;""><div style=""height:1px; background:#eceff1; line-height:1px;"">&zwnj;</div></td></tr></table>
                      <p style=""margin: 0; color: #263238;"">Việc sử dụng dịch vụ và website của chúng tôi tuân theo <a href=""#"" style=""color:#7367f0; text-decoration:none;"">Điều khoản sử dụng</a> và <a href=""#"" style=""color:#7367f0; text-decoration:none;"">Chính sách bảo mật</a>.</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""ja"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>ファイルのアップロードが完了しました</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin: 0; width: 100%; padding: 0; word-break: break-word; -webkit-font-smoothing: antialiased; background-color: #eceff1;"">
  <div style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; display: none;"">ファイルのアップロードが正常に完了しました</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""ファイルのアップロードが完了しました"" lang=""ja"" style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly;"">
    <table style=""width: 100%; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""background-color: #eceff1; font-family: Montserrat, -apple-system, 'Segoe UI', sans-serif;"">
          <table style=""width: 600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr><td style=""padding: 48px; text-align: center;""></td></tr>
            <tr>
              <td align=""center"">
                <table style=""width: 100%;"" role=""presentation"">
                  <tr>
                    <td style=""border-radius: 4px; background-color: #ffffff; padding: 48px; text-align: left; font-size: 16px; line-height: 24px; color: #626262;"">
                      <p style=""margin: 0; font-size: 20px; font-weight: 600;"">こんにちは</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}様！</p>
                      <p style=""margin: 16px 0 8px; font-size: 20px; font-weight: 600; color: #263238;"">ファイルのアップロードが正常に完了しました。</p>
                      <div style=""background-color: #f8fafc; border: 1px solid #e2e8f0; color: #334155; padding: 14px 16px; border-radius: 6px; margin: 16px 0;"">
                        <strong>ファイル詳細</strong><br>
                        • ファイル名: {{FileName}}<br>
                        • ファイルサイズ: {{FileSize}}<br>
                        • アップロード時刻: {{UploadTime}}<br>
                        • IPアドレス: {{IpAddress}}
                      </div>
                      <p style=""margin: 0;"">サービスをご利用いただき、ありがとうございます！</p>
                      <table style=""width: 100%;"" role=""presentation""><tr><td style=""padding-top: 32px; padding-bottom: 32px;""><div style=""height:1px; background:#eceff1; line-height:1px;"">&zwnj;</div></td></tr></table>
                      <p style=""margin: 0; color: #263238;"">当社のサービスとウェブサイトのご利用は <a href=""#"" style=""color:#7367f0; text-decoration:none;"">利用規約</a> と <a href=""#"" style=""color:#7367f0; text-decoration:none;"">プライバシーポリシー</a> に従います。</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""en"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>File Downloaded Successfully</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin:0;width:100%;padding:0;word-break:break-word;-webkit-font-smoothing:antialiased;background-color:#eceff1;"">
  <div style=""font-family:'Montserrat',sans-serif;mso-line-height-rule:exactly;display:none;"">Your file was downloaded successfully</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""File Downloaded Successfully"" lang=""en"" style=""font-family:'Montserrat',sans-serif;mso-line-height-rule:exactly;"">
    <table style=""width:100%;font-family:Montserrat,-apple-system,'Segoe UI',sans-serif;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""background-color:#eceff1;font-family:Montserrat,-apple-system,'Segoe UI',sans-serif;"">
          <table class=""sm-w-full"" style=""width:600px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr><td style=""padding:48px;text-align:center;""></td></tr>
            <tr>
              <td align=""center"" class=""sm-px-24"">
                <table style=""width:100%;"" role=""presentation"">
                  <tr>
                    <td style=""border-radius:4px;background:#fff;padding:48px;text-align:left;font-size:16px;line-height:24px;color:#626262;"">
                      <p style=""margin:0;font-size:20px;font-weight:600;"">Hello</p>
                      <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                      <p style=""margin:16px 0 8px;font-size:20px;font-weight:600;color:#263238;"">Your file has been downloaded successfully.</p>
                      <div style=""background:#f8fafc;border:1px solid #e2e8f0;color:#334155;padding:14px 16px;border-radius:6px;margin:16px 0;"">
                        <strong>File Details</strong><br>
                        • File Name: {{FileName}}<br>
                        • File Size: {{FileSize}}<br>
                        • Download Time: {{DownloadTime}}<br>
                        • IP Address: {{IpAddress}}
                      </div>
                      <p style=""margin:0;"">Thank you for using our service!</p>
                      <table style=""width:100%;"" role=""presentation""><tr><td style=""padding-top:32px;padding-bottom:32px;""><div style=""height:1px;background:#eceff1;line-height:1px;"">&zwnj;</div></td></tr></table>
                      <p style=""margin:0;color:#263238;"">Use of our service and website is subject to our <a href=""#"" style=""color:#7367f0;text-decoration:none;"">Terms of Use</a> and <a href=""#"" style=""color:#7367f0;text-decoration:none;"">Privacy Policy</a>.</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""vi"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>Tệp đã được tải xuống thành công</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin:0;width:100%;padding:0;word-break:break-word;-webkit-font-smoothing:antialiased;background-color:#eceff1;"">
  <div style=""font-family:'Montserrat',sans-serif;mso-line-height-rule:exactly;display:none;"">Tệp của bạn đã được tải xuống thành công</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Tệp đã được tải xuống thành công"" lang=""vi"" style=""font-family:'Montserrat',sans-serif;mso-line-height-rule:exactly;"">
    <table style=""width:100%;font-family:Montserrat,-apple-system,'Segoe UI',sans-serif;"" role=""presentation"">
      <tr>
        <td align=""center"" style=""background:#eceff1;"">
          <table style=""width:600px;"" role=""presentation"">
            <tr><td style=""padding:48px;text-align:center;""></td></tr>
            <tr>
              <td>
                <table style=""width:100%;"" role=""presentation"">
                  <tr>
                    <td style=""border-radius:4px;background:#fff;padding:48px;text-align:left;font-size:16px;line-height:24px;color:#626262;"">
                      <p style=""margin:0;font-size:20px;font-weight:600;"">Xin chào</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                      <p style=""margin:16px 0 8px;font-size:20px;font-weight:600;color:#263238;"">Tệp của bạn đã được tải xuống thành công.</p>
                      <div style=""background:#f8fafc;border:1px solid #e2e8f0;color:#334155;padding:14px 16px;border-radius:6px;margin:16px 0;"">
                        <strong>Chi tiết tệp</strong><br>
                        • Tên tệp: {{FileName}}<br>
                        • Kích thước: {{FileSize}}<br>
                        • Thời gian tải xuống: {{DownloadTime}}<br>
                        • Địa chỉ IP: {{IpAddress}}
                      </div>
                      <p style=""margin:0;"">Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                      <table style=""width:100%;"" role=""presentation""><tr><td style=""padding-top:32px;padding-bottom:32px;""><div style=""height:1px;background:#eceff1;line-height:1px;"">&zwnj;</div></td></tr></table>
                      <p style=""margin:0;color:#263238;"">Việc sử dụng dịch vụ và website của chúng tôi tuân theo <a href=""#"" style=""color:#7367f0;text-decoration:none;"">Điều khoản sử dụng</a> và <a href=""#"" style=""color:#7367f0;text-decoration:none;"">Chính sách bảo mật</a>.</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""ja"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>ファイルのダウンロードが完了しました</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin:0;width:100%;padding:0;word-break:break-word;-webkit-font-smoothing:antialiased;background-color:#eceff1;"">
  <div style=""font-family:'Montserrat',sans-serif;mso-line-height-rule:exactly;display:none;"">ファイルのダウンロードが正常に完了しました</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""ファイルのダウンロードが完了しました"" lang=""ja"" style=""font-family:'Montserrat',sans-serif;mso-line-height-rule:exactly;"">
    <table style=""width:100%;font-family:Montserrat,-apple-system,'Segoe UI',sans-serif;"" role=""presentation"">
      <tr>
        <td align=""center"" style=""background:#eceff1;"">
          <table style=""width:600px;"" role=""presentation"">
            <tr><td style=""padding:48px;text-align:center;""></td></tr>
            <tr>
              <td>
                <table style=""width:100%;"" role=""presentation"">
                  <tr>
                    <td style=""border-radius:4px;background:#fff;padding:48px;text-align:left;font-size:16px;line-height:24px;color:#626262;"">
                      <p style=""margin:0;font-size:20px;font-weight:600;"">こんにちは</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}様！</p>
                      <p style=""margin:16px 0 8px;font-size:20px;font-weight:600;color:#263238;"">ファイルのダウンロードが正常に完了しました。</p>
                      <div style=""background:#f8fafc;border:1px solid #e2e8f0;color:#334155;padding:14px 16px;border-radius:6px;margin:16px 0;"">
                        <strong>ファイル詳細</strong><br>
                        • ファイル名: {{FileName}}<br>
                        • ファイルサイズ: {{FileSize}}<br>
                        • ダウンロード時刻: {{DownloadTime}}<br>
                        • IPアドレス: {{IpAddress}}
                      </div>
                      <p style=""margin:0;"">サービスをご利用いただき、ありがとうございます！</p>
                      <table style=""width:100%;"" role=""presentation""><tr><td style=""padding-top:32px;padding-bottom:32px;""><div style=""height:1px;background:#eceff1;line-height:1px;"">&zwnj;</div></td></tr></table>
                      <p style=""margin:0;color:#263238;"">当社のサービスとウェブサイトのご利用は <a href=""#"" style=""color:#7367f0;text-decoration:none;"">利用規約</a> と <a href=""#"" style=""color:#7367f0;text-decoration:none;"">プライバシーポリシー</a> に従います。</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""en"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>File Deleted Successfully</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin:0;width:100%;padding:0;word-break:break-word;-webkit-font-smoothing:antialiased;background-color:#eceff1;"">
  <div style=""font-family:'Montserrat',sans-serif;mso-line-height-rule:exactly;display:none;"">Your file was deleted successfully</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""File Deleted Successfully"" lang=""en"" style=""font-family:'Montserrat',sans-serif;mso-line-height-rule:exactly;"">
    <table style=""width:100%;font-family:Montserrat,-apple-system,'Segoe UI',sans-serif;"" role=""presentation"">
      <tr>
        <td align=""center"" style=""background:#eceff1;"">
          <table style=""width:600px;"" role=""presentation"">
            <tr><td style=""padding:48px;text-align:center;""></td></tr>
            <tr>
              <td>
                <table style=""width:100%;"" role=""presentation"">
                  <tr>
                    <td style=""border-radius:4px;background:#fff;padding:48px;text-align:left;font-size:16px;line-height:24px;color:#626262;"">
                      <p style=""margin:0;font-size:20px;font-weight:600;"">Hello</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                      <p style=""margin:16px 0 8px;font-size:20px;font-weight:600;color:#263238;"">Your file has been deleted successfully.</p>
                      <div style=""background:#f8fafc;border:1px solid #e2e8f0;color:#334155;padding:14px 16px;border-radius:6px;margin:16px 0;"">
                        <strong>File Details</strong><br>
                        • File Name: {{FileName}}<br>
                        • File Size: {{FileSize}}<br>
                        • Delete Time: {{DeleteTime}}<br>
                        • IP Address: {{IpAddress}}
                      </div>
                      <p style=""margin:0;"">Thank you for using our service!</p>
                      <table style=""width:100%;"" role=""presentation""><tr><td style=""padding-top:32px;padding-bottom:32px;""><div style=""height:1px;background:#eceff1;line-height:1px;"">&zwnj;</div></td></tr></table>
                      <p style=""margin:0;color:#263238;"">Use of our service and website is subject to our <a href=""#"" style=""color:#7367f0;text-decoration:none;"">Terms of Use</a> and <a href=""#"" style=""color:#7367f0;text-decoration:none;"">Privacy Policy</a>.</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""vi"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>Tệp đã được xóa thành công</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin:0;width:100%;padding:0;word-break:break-word;-webkit-font-smoothing:antialiased;background-color:#eceff1;"">
  <div style=""font-family:'Montserrat',sans-serif;mso-line-height-rule:exactly;display:none;"">Tệp của bạn đã được xóa thành công</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""Tệp đã được xóa thành công"" lang=""vi"" style=""font-family:'Montserrat',sans-serif;mso-line-height-rule:exactly;"">
    <table style=""width:100%;font-family:Montserrat,-apple-system,'Segoe UI',sans-serif;"" role=""presentation"">
      <tr>
        <td align=""center"" style=""background:#eceff1;"">
          <table style=""width:600px;"" role=""presentation"">
            <tr><td style=""padding:48px;text-align:center;""></td></tr>
            <tr>
              <td>
                <table style=""width:100%;"" role=""presentation"">
                  <tr>
                    <td style=""border-radius:4px;background:#fff;padding:48px;text-align:left;font-size:16px;line-height:24px;color:#626262;"">
                      <p style=""margin:0;font-size:20px;font-weight:600;"">Xin chào</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}!</p>
                      <p style=""margin:16px 0 8px;font-size:20px;font-weight:600;color:#263238;"">Tệp của bạn đã được xóa thành công.</p>
                      <div style=""background:#f8fafc;border:1px solid #e2e8f0;color:#334155;padding:14px 16px;border-radius:6px;margin:16px 0;"">
                        <strong>Chi tiết tệp</strong><br>
                        • Tên tệp: {{FileName}}<br>
                        • Kích thước: {{FileSize}}<br>
                        • Thời gian xóa: {{DeleteTime}}<br>
                        • Địa chỉ IP: {{IpAddress}}
                      </div>
                      <p style=""margin:0;"">Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                      <table style=""width:100%;"" role=""presentation""><tr><td style=""padding-top:32px;padding-bottom:32px;""><div style=""height:1px;background:#eceff1;line-height:1px;"">&zwnj;</div></td></tr></table>
                      <p style=""margin:0;color:#263238;"">Việc sử dụng dịch vụ và website của chúng tôi tuân theo <a href=""#"" style=""color:#7367f0;text-decoration:none;"">Điều khoản sử dụng</a> và <a href=""#"" style=""color:#7367f0;text-decoration:none;"">Chính sách bảo mật</a>.</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
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
<html lang=""ja"">
<head>
  <meta charset=""utf-8"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"">
  <title>ファイルの削除が完了しました</title>
  <link href=""https://fonts.googleapis.com/css?family=Montserrat:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,200;1,300;1,400;1,500;1,600;1,700"" rel=""stylesheet"" media=""screen"">
</head>
<body style=""margin:0;width:100%;padding:0;word-break:break-word;-webkit-font-smoothing:antialiased;background-color:#eceff1;"">
  <div style=""font-family:'Montserrat',sans-serif;mso-line-height-rule:exactly;display:none;"">ファイルの削除が正常に完了しました</div>
  <div role=""article"" aria-roledescription=""email"" aria-label=""ファイルの削除が完了しました"" lang=""ja"" style=""font-family:'Montserrat',sans-serif;mso-line-height-rule:exactly;"">
    <table style=""width:100%;font-family:Montserrat,-apple-system,'Segoe UI',sans-serif;"" role=""presentation"">
      <tr>
        <td align=""center"" style=""background:#eceff1;"">
          <table style=""width:600px;"" role=""presentation"">
            <tr><td style=""padding:48px;text-align:center;""></td></tr>
            <tr>
              <td>
                <table style=""width:100%;"" role=""presentation"">
                  <tr>
                    <td style=""border-radius:4px;background:#fff;padding:48px;text-align:left;font-size:16px;line-height:24px;color:#626262;"">
                      <p style=""margin:0;font-size:20px;font-weight:600;"">こんにちは</p>
                        <p style=""font-family: 'Montserrat', sans-serif; mso-line-height-rule: exactly; margin-top: 0; font-size: 24px; font-weight: 700; color:rgb(215, 31, 188);"">{{Username}}様！</p>
                      <p style=""margin:16px 0 8px;font-size:20px;font-weight:600;color:#263238;"">ファイルの削除が正常に完了しました。</p>
                      <div style=""background:#f8fafc;border:1px solid #e2e8f0;color:#334155;padding:14px 16px;border-radius:6px;margin:16px 0;"">
                        <strong>ファイル詳細</strong><br>
                        • ファイル名: {{FileName}}<br>
                        • ファイルサイズ: {{FileSize}}<br>
                        • 削除時刻: {{DeleteTime}}<br>
                        • IPアドレス: {{IpAddress}}
                      </div>
                      <p style=""margin:0;"">サービスをご利用いただき、ありがとうございます！</p>
                      <table style=""width:100%;"" role=""presentation""><tr><td style=""padding-top:32px;padding-bottom:32px;""><div style=""height:1px;background:#eceff1;line-height:1px;"">&zwnj;</div></td></tr></table>
                      <p style=""margin:0;color:#263238;"">当社のサービスとウェブサイトのご利用は <a href=""#"" style=""color:#7367f0;text-decoration:none;"">利用規約</a> と <a href=""#"" style=""color:#7367f0;text-decoration:none;"">プライバシーポリシー</a> に従います。</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
</body>
</html>",
                Description = "ファイル削除通知用テンプレート",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
