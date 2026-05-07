USE [db_ac8a39_lamchubaotoan]
GO

-- Khai báo biến chứa câu lệnh SQL động
DECLARE @Sql NVARCHAR(MAX) = '';

-- =========================================================================
-- BƯỚC 1: Xóa tất cả các ràng buộc Khóa ngoại (Foreign Keys) để tránh lỗi
-- =========================================================================
SELECT @Sql += 'ALTER TABLE ' 
    + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' 
    + QUOTENAME(OBJECT_NAME(parent_object_id)) 
    + ' DROP CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
FROM sys.foreign_keys;

-- Thực thi lệnh xóa Khóa ngoại
IF LEN(@Sql) > 0
BEGIN
    PRINT 'Đang xóa các khóa ngoại (Foreign Keys)...'
    EXEC sp_executesql @Sql;
END

-- =========================================================================
-- BƯỚC 2: Xóa tất cả các bảng (Tables)
-- =========================================================================
-- Reset lại biến @Sql
SET @Sql = '';

SELECT @Sql += 'DROP TABLE ' 
    + QUOTENAME(TABLE_SCHEMA) + '.' 
    + QUOTENAME(TABLE_NAME) + ';' + CHAR(13)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';

-- Thực thi lệnh xóa Bảng
IF LEN(@Sql) > 0
BEGIN
    PRINT 'Đang xóa các bảng (Tables)...'
    EXEC sp_executesql @Sql;
END

PRINT 'Đã xóa thành công toàn bộ các bảng trong database!'
GO