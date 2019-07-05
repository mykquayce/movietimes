CREATE TABLE `cineworld`.`log` (
  `timeStamp` TIMESTAMP,
  `lastModified` DATETIME NOT NULL UNIQUE CHECK (`lastModified` <= `timeStamp`),
  PRIMARY KEY (`lastModified`),
  INDEX `idx_cineworld_log_lastModified` (`lastModified` DESC)
);
