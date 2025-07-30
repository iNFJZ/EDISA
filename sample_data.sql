INSERT INTO "Users" (
    "Id", "Username", "FullName", "Email", "PhoneNumber", "Status", "IsVerified",
    "LastLoginAt", "DateOfBirth", "Address", "Bio", "LoginProvider",
    "ProfilePicture", "CreatedAt", "UpdatedAt", "DeletedAt"
) VALUES
(gen_random_uuid(), 'admin', 'Admin Root', 'admin@edisa.com', '0901110001', 1, TRUE, '2025-07-17 08:54:20.754+07', '1990-01-01', 'Hà Nội', 'Super admin', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'user', 'Tien Dung', 'user@edisa.com', '0901110001', 1, TRUE, '2025-07-17 08:54:20.754+07', '1990-01-01', 'Hà Nội', 'Super user', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
-- 15 users: status = 4, DeletedAt != null
(gen_random_uuid(), 'alice', 'Alice Johnson', 'alice.johnson@example.com', '0901110002', 4, TRUE, '2025-07-17 08:54:20.754+07', '1992-03-03', 'Hồ Chí Minh', 'HR manager', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'bob', 'Bob Smith', 'bob.smith@example.com', '0901110003', 4, TRUE, '2025-07-17 08:54:20.754+07', '1988-04-04', 'Đà Nẵng', 'IT support', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'carol', 'Carol Lee', 'carol.lee@example.com', '0901110004', 4, TRUE, '2025-07-17 08:54:20.754+07', '1995-05-05', 'Cần Thơ', 'Marketing lead', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'david', 'David Kim', 'david.kim@example.com', '0901110005', 4, TRUE, '2025-07-17 08:54:20.754+07', '1991-06-06', 'Hải Phòng', 'Sales', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'eva', 'Eva Green', 'eva.green@example.com', '0901110006', 4, TRUE, '2025-07-17 08:54:20.754+07', '1993-07-07', 'Huế', 'Designer', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'frank', 'Frank Miller', 'frank.miller@example.com', '0901110007', 4, TRUE, '2025-07-17 08:54:20.754+07', '1987-08-08', 'Nha Trang', 'QA engineer', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'grace', 'Grace Park', 'grace.park@example.com', '0901110008', 4, TRUE, '2025-07-17 08:54:20.754+07', '1994-09-09', 'Vũng Tàu', 'Business analyst', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'henry', 'Henry Ford', 'henry.ford@example.com', '0901110009', 4, TRUE, '2025-07-17 08:54:20.754+07', '1996-10-10', 'Quảng Ninh', 'Logistics', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'irene', 'Irene Adler', 'irene.adler@example.com', '0901110010', 4, TRUE, '2025-07-17 08:54:20.754+07', '1997-11-11', 'Bắc Ninh', 'Content writer', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'jack', 'Jack Ma', 'jack.ma@example.com', '0901110011', 4, TRUE, '2025-07-17 08:54:20.754+07', '1989-12-12', 'Nam Định', 'CEO', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'kate', 'Kate Moss', 'kate.moss@example.com', '0901110012', 4, TRUE, '2025-07-17 08:54:20.754+07', '1998-01-13', 'Thái Bình', 'PR specialist', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'leo', 'Leo Messi', 'leo.messi@example.com', '0901110013', 4, TRUE, '2025-07-17 08:54:20.754+07', '1990-02-14', 'Thanh Hoá', 'Footballer', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'mia', 'Mia Wong', 'mia.wong@example.com', '0901110014', 4, TRUE, '2025-07-17 08:54:20.754+07', '1992-03-15', 'Bình Dương', 'Accountant', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'nick', 'Nick Fury', 'nick.fury@example.com', '0901110015', 4, TRUE, '2025-07-17 08:54:20.754+07', '1993-04-16', 'Bình Phước', 'Security', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
(gen_random_uuid(), 'olivia', 'Olivia Brown', 'olivia.brown@example.com', '0901110016', 4, TRUE, '2025-07-17 08:54:20.754+07', '1994-05-17', 'Phú Thọ', 'Teacher', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', '2025-07-18 08:54:20.754+07'),
-- 10 users: status = 3
(gen_random_uuid(), 'peter', 'Peter Parker', 'peter.parker@example.com', '0901110017', 3, TRUE, '2025-07-17 08:54:20.754+07', '1991-06-18', 'Hà Nam', 'Photographer', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'quinn', 'Quinn Tran', 'quinn.tran@example.com', '0901110018', 3, TRUE, '2025-07-17 08:54:20.754+07', '1992-07-19', 'Hưng Yên', 'Developer', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'rachel', 'Rachel Adams', 'rachel.adams@example.com', '0901110019', 3, TRUE, '2025-07-17 08:54:20.754+07', '1993-08-20', 'Lào Cai', 'Nurse', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'sam', 'Sam Wilson', 'sam.wilson@example.com', '0901110020', 3, TRUE, '2025-07-17 08:54:20.754+07', '1994-09-21', 'Bắc Giang', 'Pilot', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'tina', 'Tina Nguyen', 'tina.nguyen@example.com', '0901110021', 3, TRUE, '2025-07-17 08:54:20.754+07', '1995-10-22', 'Bắc Kạn', 'Chef', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'ursula', 'Ursula Pham', 'ursula.pham@example.com', '0901110022', 3, TRUE, '2025-07-17 08:54:20.754+07', '1996-11-23', 'Lạng Sơn', 'Receptionist', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'victor', 'Victor Hugo', 'victor.hugo@example.com', '0901110023', 3, TRUE, '2025-07-17 08:54:20.754+07', '1997-12-24', 'Sơn La', 'Writer', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'wendy', 'Wendy Do', 'wendy.do@example.com', '0901110024', 3, TRUE, '2025-07-17 08:54:20.754+07', '1998-01-25', 'Tuyên Quang', 'Student', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'xander', 'Xander Zhou', 'xander.zhou@example.com', '0901110025', 3, TRUE, '2025-07-17 08:54:20.754+07', '1999-02-26', 'Yên Bái', 'Musician', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'yara', 'Yara Lin', 'yara.lin@example.com', '0901110026', 3, TRUE, '2025-07-17 08:54:20.754+07', '2000-03-27', 'Hòa Bình', 'Dancer', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
-- 5 users: status = 2, isVerified = false
(gen_random_uuid(), 'zane', 'Zane Black', 'zane.black@example.com', '0901110027', 2, FALSE, '2025-07-17 08:54:20.754+07', '1991-04-28', 'Quảng Bình', 'Mechanic', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'amy', 'Amy White', 'amy.white@example.com', '0901110028', 2, FALSE, '2025-07-17 08:54:20.754+07', '1992-05-01', 'Bình Định', 'Florist', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'brian', 'Brian Red', 'brian.red@example.com', '0901110029', 2, FALSE, '2025-07-17 08:54:20.754+07', '1993-06-02', 'Phan Thiết', 'Barista', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'claire', 'Claire Blue', 'claire.blue@example.com', '0901110030', 2, FALSE, '2025-07-17 08:54:20.754+07', '1994-07-03', 'Vĩnh Long', 'Nanny', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL),
(gen_random_uuid(), 'dan', 'Dan Green', 'dan.green@example.com', '0901110031', 2, FALSE, '2025-07-17 08:54:20.754+07', '1995-08-04', 'Long An', 'Driver', 'Local', NULL, '2025-07-17 08:53:17.542+07', '2025-07-17 08:54:20.754+07', NULL);

UPDATE "Users" 
SET "PasswordHash" = '$2y$10$NM9q4XS3wI7lzYi6fPkFeewJutyiI0ydik89.LAtDY4rpCKCKmJ4W' 
WHERE "Email" = 'admin@edisa.com';

UPDATE "Users" 
SET "PasswordHash" = '$2y$10$NM9q4XS3wI7lzYi6fPkFeewJutyiI0ydik89.LAtDY4rpCKCKmJ4W' 
WHERE "Email" = 'user@edisa.com';

--Mat khau la Testtest1

-- Insert welcome notification for all users
INSERT INTO "Notifications" (
    "UserId", "Title", "Message", "Type", "Icon", "IsRead", "CreatedAt", "ReadAt", "Data"
) VALUES 
-- Notification for admin user
('admin', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
-- Notification for user
('user', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
-- Notifications for all other users (status 2, 3, 4)
('alice', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('bob', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('carol', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('david', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('eva', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('frank', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('grace', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('henry', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('irene', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('jack', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('kate', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('leo', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('mia', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('nick', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('olivia', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('peter', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('quinn', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('rachel', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('sam', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('tina', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('ursula', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('victor', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('wendy', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('xander', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('yara', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('zane', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('amy', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('brian', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('claire', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}'),
('dan', 'Welcome to EDISA', 'Chào mừng bạn đến với hệ thống EDISA! Chúng tôi rất vui mừng được phục vụ bạn.', 'info', 'ti ti-bell', FALSE, NOW(), NULL, '{"actionUrl": "/admin/dashboard", "actionText": "Go to Dashboard"}');