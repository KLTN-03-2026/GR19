-- Đảm bảo không có ai đang kết nối vào DB để tránh lỗi
USE master;
GO

-- Lệnh Backup database
BACKUP DATABASE [CafebookDB]
TO DISK = N'E:\Tai Lieu Hoc Tap\N19 KLTN 032026\Cafebook\DatabaseCafebook\CafebookDBbackup.bak'
WITH FORMAT,
     MEDIANAME = 'SQLServerBackups',
     NAME = 'Full Backup of CafebookDB',
     STATS = 1; -- Hiển thị tiến trình mỗi 10%
GO