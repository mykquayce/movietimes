CREATE TABLE `moviedetails`.`movie` (
  `imdbId` INT UNSIGNED NULL,
  `title` VARCHAR(100) NOT NULL,
  `runtime` TINYINT UNSIGNED NOT NULL,
  PRIMARY KEY (`imdbId`)
);
