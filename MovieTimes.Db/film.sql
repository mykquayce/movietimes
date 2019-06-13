CREATE TABLE `cineworld`.`film` (
  `edi` INT UNSIGNED NOT NULL,
  `title` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`edi`),
  UNIQUE INDEX `uix_cineworld_film` (`title`)
);
