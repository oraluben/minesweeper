USE Mine;
IF OBJECT_ID('dbo.MineGroup', 'U') IS NOT NULL
  DROP TABLE dbo.MineGroup; 
CREATE TABLE dbo.MineGroup (
  x INT NOT NULL,
  y INT NOT NULL,
  bin BINARY(8000) NOT NULL,
  PRIMARY KEY (x, y)
);