﻿DROP TABLE IF EXISTS DashboardDB;

CREATE TABLE DashboardDB (
[ID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
[NAME] VARCHAR(15) NOT NULL,
[IMG_PATH] VARCHAR(50) NOT NULL,
[aspectRatio] VARCHAR(6) NOT NULL
);

INSERT INTO DashboardDB (NAME, IMG_PATH, aspectRatio) VALUES ('Board_TEST01', 'images/test.jpg', '1.7777');