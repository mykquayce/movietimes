CREATE TABLE `moviedetails`.`mapping` (
  `edi` INT UNSIGNED NULL,
  `id` INT UNSIGNED NULL,
  `formats` SMALLINT UNSIGNED NULL,
  PRIMARY KEY (`edi`),
  UNIQUE INDEX `uix_moviedetails_mapping1` (`edi`, `id`, `formats`),
  UNIQUE INDEX `uix_moviedetails_mapping2` (`edi`, `formats`),
  FOREIGN KEY (`edi`)
    REFERENCES `cineworld`.`film`(`edi`)
    ON DELETE CASCADE,
  FOREIGN KEY (`id`)
    REFERENCES `moviedetails`.`movie`(`id`)
    ON DELETE CASCADE
);
