-- MySQL dump 10.13  Distrib 5.7.19, for Win64 (x86_64)
--
-- Host: localhost    Database: jzfp2020
-- ------------------------------------------------------
-- Server version	5.7.19

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `fphistorydata`
--

DROP TABLE IF EXISTS `fphistorydata`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `fphistorydata` (
  `NO` int(11) NOT NULL AUTO_INCREMENT,
  `Xzj` longtext CHARACTER SET utf8mb4,
  `Csq` longtext CHARACTER SET utf8mb4,
  `Address` longtext CHARACTER SET utf8mb4,
  `Name` longtext CHARACTER SET utf8mb4,
  `idcard` varchar(18) NOT NULL,
  `BirthDay` longtext CHARACTER SET utf8mb4,
  `Pkrk` longtext CHARACTER SET utf8mb4,
  `PkrkDate` longtext CHARACTER SET utf8mb4,
  `Tkry` longtext CHARACTER SET utf8mb4,
  `TkryDate` longtext CHARACTER SET utf8mb4,
  `Qedb` longtext CHARACTER SET utf8mb4,
  `QedbDate` longtext CHARACTER SET utf8mb4,
  `Cedb` longtext CHARACTER SET utf8mb4,
  `CedbDate` longtext CHARACTER SET utf8mb4,
  `Yejc` longtext CHARACTER SET utf8mb4,
  `YejcDate` longtext CHARACTER SET utf8mb4,
  `Ssjc` longtext CHARACTER SET utf8mb4,
  `SsjcDate` longtext CHARACTER SET utf8mb4,
  `Sypkry` longtext CHARACTER SET utf8mb4,
  `Jbrdsf` longtext CHARACTER SET utf8mb4,
  `JbrdsfFirstDate` longtext CHARACTER SET utf8mb4,
  `JbrdsfLastDate` longtext CHARACTER SET utf8mb4,
  `Jbcbqk` longtext CHARACTER SET utf8mb4,
  `JbcbqkDate` longtext CHARACTER SET utf8mb4,
  PRIMARY KEY (`NO`),
  UNIQUE KEY `UNQ_Idcard` (`idcard`)
) ENGINE=InnoDB AUTO_INCREMENT=27576 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fpmonthdata`
--

DROP TABLE IF EXISTS `fpmonthdata`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `fpmonthdata` (
  `NO` int(11) NOT NULL AUTO_INCREMENT,
  `Xzj` longtext CHARACTER SET utf8mb4,
  `Csq` longtext CHARACTER SET utf8mb4,
  `Address` longtext CHARACTER SET utf8mb4,
  `Name` longtext CHARACTER SET utf8mb4,
  `idcard` varchar(18) NOT NULL,
  `BirthDay` longtext CHARACTER SET utf8mb4,
  `Pkrk` longtext CHARACTER SET utf8mb4,
  `PkrkDate` longtext CHARACTER SET utf8mb4,
  `Tkry` longtext CHARACTER SET utf8mb4,
  `TkryDate` longtext CHARACTER SET utf8mb4,
  `Qedb` longtext CHARACTER SET utf8mb4,
  `QedbDate` longtext CHARACTER SET utf8mb4,
  `Cedb` longtext CHARACTER SET utf8mb4,
  `CedbDate` longtext CHARACTER SET utf8mb4,
  `Yejc` longtext CHARACTER SET utf8mb4,
  `YejcDate` longtext CHARACTER SET utf8mb4,
  `Ssjc` longtext CHARACTER SET utf8mb4,
  `SsjcDate` longtext CHARACTER SET utf8mb4,
  `Sypkry` longtext CHARACTER SET utf8mb4,
  `Jbrdsf` longtext CHARACTER SET utf8mb4,
  `JbrdsfFirstDate` longtext CHARACTER SET utf8mb4,
  `JbrdsfLastDate` longtext CHARACTER SET utf8mb4,
  `Jbcbqk` longtext CHARACTER SET utf8mb4,
  `JbcbqkDate` longtext CHARACTER SET utf8mb4,
  `month` varchar(6) DEFAULT NULL,
  PRIMARY KEY (`NO`),
  UNIQUE KEY `UNQ_Idcard` (`idcard`,`month`)
) ENGINE=InnoDB AUTO_INCREMENT=262233 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fprawdata`
--

DROP TABLE IF EXISTS `fprawdata`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `fprawdata` (
  `NO` int(11) NOT NULL AUTO_INCREMENT,
  `Xzj` longtext CHARACTER SET utf8mb4,
  `Csq` longtext CHARACTER SET utf8mb4,
  `Address` longtext CHARACTER SET utf8mb4,
  `Name` longtext CHARACTER SET utf8mb4,
  `idcard` varchar(18) NOT NULL,
  `BirthDay` longtext CHARACTER SET utf8mb4,
  `type` varchar(100) DEFAULT NULL,
  `Detail` longtext CHARACTER SET utf8mb4,
  `date` varchar(6) DEFAULT NULL,
  PRIMARY KEY (`NO`),
  UNIQUE KEY `UNQ_Idcard` (`idcard`,`date`,`type`)
) ENGINE=InnoDB AUTO_INCREMENT=319334 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `jbrymx`
--

DROP TABLE IF EXISTS `jbrymx`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `jbrymx` (
  `idcard` varchar(18) NOT NULL,
  `Xzqh` longtext CHARACTER SET utf8mb4,
  `Hjxz` longtext CHARACTER SET utf8mb4,
  `Name` longtext CHARACTER SET utf8mb4,
  `Sex` longtext CHARACTER SET utf8mb4,
  `BirthDay` longtext CHARACTER SET utf8mb4,
  `Cbsf` longtext CHARACTER SET utf8mb4,
  `Cbzt` longtext CHARACTER SET utf8mb4,
  `Jfzt` longtext CHARACTER SET utf8mb4,
  `Cbsj` longtext CHARACTER SET utf8mb4,
  PRIMARY KEY (`idcard`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-12-30 16:47:13
