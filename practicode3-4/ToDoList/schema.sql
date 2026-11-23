-- יצירת הטבלאות במסד נתונים חדש (למשל מסד בענן)
CREATE TABLE IF NOT EXISTS users (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(100) NOT NULL,
    Password VARCHAR(255) NOT NULL,
    UNIQUE INDEX IX_users_Username (Username)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS items (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NULL,
    IsComplete TINYINT(1) NULL,
    UserId INT NULL,
    INDEX IX_items_UserId (UserId),
    CONSTRAINT FK_items_users_UserId FOREIGN KEY (UserId) REFERENCES users (Id) ON DELETE CASCADE
) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
