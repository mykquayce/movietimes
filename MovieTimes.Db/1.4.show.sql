CREATE TABLE `cineworld`.`show` (
  `cinemaId` SMALLINT(2) UNSIGNED NOT NULL,
  `filmEdi` INT UNSIGNED NOT NULL,
  `time` DATETIME NOT NULL,
  PRIMARY KEY (`cinemaId`, `filmEdi`, `time`),
    FOREIGN KEY fk_cinemaId_film_edi(`cinemaId`)
    REFERENCES `cineworld`.`cinema` (`id`)
    ON DELETE CASCADE
    ON UPDATE NO ACTION,
    FOREIGN KEY fk_filmEdi_film_edi(`filmEdi`)
    REFERENCES `cineworld`.`film` (`edi`)
    ON DELETE CASCADE
    ON UPDATE NO ACTION
);
