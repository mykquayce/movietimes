CREATE TABLE `cineworld`.`cinema` (
  `id` SMALLINT(2) UNSIGNED NOT NULL,
  `name` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `uix_cineworld_cinema` (`name`)
);
