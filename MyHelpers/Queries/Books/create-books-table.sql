CREATE TABLE books (
    ID INT NOT NULL PRIMARY KEY IDENTITY,
    Title VARCHAR(150) NOT NULL,
    Authors VARCHAR(150) NOT NULL,
    Isbn VARCHAR(50) NOT NULL,
    Pages VARCHAR(25) NOT NULL,
    Price DECIMAL (16,2) NOT NULL,
    Category VARCHAR(25) NOT NULL,
    Description TEXT NOT NULL,
    Image_filename VARCHAR(255) NOT NULL,
    Created_on DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, 
);