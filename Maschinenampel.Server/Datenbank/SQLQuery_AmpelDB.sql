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
    (1, 1, 20, 5, 3, '["rot", "gelb", "grün"]', '["1", "0", "0"]'),
    (2, 1, 40, 5, 2, '["blau", "weiß"]', '["0", "1"]');