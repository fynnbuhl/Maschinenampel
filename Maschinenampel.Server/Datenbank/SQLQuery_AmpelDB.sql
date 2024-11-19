CREATE TABLE AmpelDB (
    [ID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [DASHBOARD_ID] INT NOT NULL,
    [POS_X] INT NOT NULL,
    [POS_Y] INT NOT NULL,
    [SIZE] INT NOT NULL,
    [ColorCount] INT NOT NULL,
    [COLORS] TEXT NOT NULL,
    [OPC_BIT] TEXT NOT NULL
);

INSERT INTO AmpelDB (DASHBOARD_ID, POS_X, POS_Y, SIZE, ColorCount, COLORS, OPC_BIT)
VALUES
    (1, 20, 20, 4, 3, 'green,red,blue', '["1", "0", "0"]'),
    (2, 40, 40, 4, 2, 'blue,red', '["0", "1"]');