CREATE TABLE `queries`.`result` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,  
  `datetime` DATETIME NOT NULL,
  `queryId` SMALLINT UNSIGNED NOT NULL,
  `json` JSON NOT NULL,
  PRIMARY KEY (`id`),
  FOREIGN KEY fk_queryId_queries_saved_id(`queryId`)
    REFERENCES `queries`.`saved` (`id`)
    ON DELETE CASCADE
    ON UPDATE NO ACTION
);
