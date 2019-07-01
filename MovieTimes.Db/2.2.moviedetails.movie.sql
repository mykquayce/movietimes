CREATE TABLE `moviedetails`.`movie` (
  `id` INT UNSIGNED NULL,
  `imdbId` INT UNSIGNED NULL,
  `title` VARCHAR(100) NOT NULL,
  `runtime` TINYINT UNSIGNED NULL,
  PRIMARY KEY (`id`)
);
