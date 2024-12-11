CREATE TABLE AmpelDB (
    [ID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [DASHBOARD_ID] INT NOT NULL,
    [POS_X] INT NOT NULL,
    [POS_Y] INT NOT NULL,
    [SIZE] INT NOT NULL,
    [ColorCount] INT NOT NULL,
    [COLORS] TEXT NOT NULL,
    [OPC_Addr] TEXT NOT NULL,
    [OPC_TagList] TEXT NOT NULL
);

INSERT INTO AmpelDB (DASHBOARD_ID, POS_X, POS_Y, SIZE, ColorCount, COLORS, OPC_Addr, OPC_TagList)
VALUES
    (1, 20, 20, 2, 1, 'green', 'Beispiele für Simulation.Funktionen', 'User3');