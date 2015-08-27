-- MySQL Script generated by MySQL Workbench
-- 08/27/15 11:05:55
-- Model: New Model    Version: 1.0
-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='TRADITIONAL,ALLOW_INVALID_DATES';

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------
-- -----------------------------------------------------
-- Schema landfill
-- -----------------------------------------------------
DROP SCHEMA IF EXISTS `landfill` ;

-- -----------------------------------------------------
-- Schema landfill
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `landfill` DEFAULT CHARACTER SET latin1 ;
USE `landfill` ;

-- -----------------------------------------------------
-- Table `landfill`.`projects`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `landfill`.`projects` ;

CREATE TABLE IF NOT EXISTS `landfill`.`projects` (
  `projectId` INT(10) UNSIGNED NOT NULL COMMENT '',
  `name` VARCHAR(255) NOT NULL COMMENT '',
  `timeZone` VARCHAR(255) NOT NULL COMMENT '',
  `retrievalStartedAt` DATETIME NOT NULL COMMENT '',
  PRIMARY KEY (`projectId`)  COMMENT '',
  UNIQUE INDEX `projectId_UNIQUE` (`projectId` ASC)  COMMENT '')
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `landfill`.`entries`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `landfill`.`entries` ;

CREATE TABLE IF NOT EXISTS `landfill`.`entries` (
  `projectId` INT(10) UNSIGNED NOT NULL COMMENT '',
  `date` DATE NOT NULL COMMENT '',
  `weight` DOUBLE NOT NULL COMMENT '',
  `volume` DOUBLE NULL DEFAULT NULL COMMENT '',
  `volumeNotRetrieved` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0' COMMENT '',
  `volumeNotAvailable` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0' COMMENT '',
  `volumesUpdatedTimestamp` DATETIME NULL DEFAULT NULL COMMENT '',
  UNIQUE INDEX `projectId_date_unique` (`projectId` ASC, `date` ASC)  COMMENT '',
  INDEX `entriesProjectId` (`projectId` ASC)  COMMENT '',
  CONSTRAINT `projectId`
    FOREIGN KEY (`projectId`)
    REFERENCES `landfill`.`projects` (`projectId`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `landfill`.`users`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `landfill`.`users` ;

CREATE TABLE IF NOT EXISTS `landfill`.`users` (
  `userId` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '',
  `name` VARCHAR(255) NOT NULL COMMENT '',
  `projectsRetrievedAt` DATETIME NOT NULL COMMENT '',
  `unitsId` INT(10) UNSIGNED NULL DEFAULT NULL COMMENT '',
  PRIMARY KEY (`userId`)  COMMENT '',
  UNIQUE INDEX `name_UNIQUE` (`name` ASC)  COMMENT '')
ENGINE = InnoDB
AUTO_INCREMENT = 2881
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `landfill`.`sessions`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `landfill`.`sessions` ;

CREATE TABLE IF NOT EXISTS `landfill`.`sessions` (
  `sessionId` VARCHAR(32) NOT NULL COMMENT '',
  `userId` INT(10) UNSIGNED NOT NULL COMMENT '',
  `createdAt` DATETIME NULL DEFAULT CURRENT_TIMESTAMP COMMENT '',
  PRIMARY KEY (`sessionId`)  COMMENT '',
  UNIQUE INDEX `sessionId_UNIQUE` (`sessionId` ASC)  COMMENT '',
  INDEX `fkUserId_idx` (`userId` ASC)  COMMENT '',
  CONSTRAINT `fkUserId`
    FOREIGN KEY (`userId`)
    REFERENCES `landfill`.`users` (`userId`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `landfill`.`usersprojects`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `landfill`.`usersprojects` ;

CREATE TABLE IF NOT EXISTS `landfill`.`usersprojects` (
  `userId` INT(10) UNSIGNED NOT NULL COMMENT '',
  `projectId` INT(10) UNSIGNED NOT NULL COMMENT '',
  PRIMARY KEY (`userId`, `projectId`)  COMMENT '',
  UNIQUE INDEX `userProjectId` (`userId` ASC, `projectId` ASC)  COMMENT '',
  INDEX `fkProjectId_idx` (`projectId` ASC)  COMMENT '',
  INDEX `usersprojectsUserId` (`userId` ASC)  COMMENT '',
  CONSTRAINT `upProjectId`
    FOREIGN KEY (`projectId`)
    REFERENCES `landfill`.`projects` (`projectId`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `upUserId`
    FOREIGN KEY (`userId`)
    REFERENCES `landfill`.`users` (`userId`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
