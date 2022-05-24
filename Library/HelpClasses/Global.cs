using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Welcome_Settings
{
	//SQL scipt for generating GNU databas with tables and data to Heidi.
    public static class Global
    {
        public static string ConnectionString;
        public static string CompleteConnectionString;
		public static string sql = @"-- --------------------------------------------------------
-- Värd:                         127.0.0.1
-- Serverversion:                10.6.5-MariaDB - mariadb.org binary distribution
-- Server-OS:                    Win64
-- HeidiSQL Version:             11.3.0.6295
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumpar databasstruktur för gnu
CREATE DATABASE IF NOT EXISTS `gnu` /*!40100 DEFAULT CHARACTER SET latin1 */;
USE `gnu`;

-- Dumpar struktur för tabell gnu.bookmarks
CREATE TABLE IF NOT EXISTS `bookmarks` (
  `ID` int(11) NOT NULL,
  `Email` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Dumpar data för tabell gnu.bookmarks: ~0 rows (ungefär)
/*!40000 ALTER TABLE `bookmarks` DISABLE KEYS */;
/*!40000 ALTER TABLE `bookmarks` ENABLE KEYS */;

-- Dumpar struktur för tabell gnu.comments
CREATE TABLE IF NOT EXISTS `comments` (
  `ID` int(11) NOT NULL,
  `Email` varchar(50) NOT NULL DEFAULT '',
  `userName` varchar(50) DEFAULT NULL,
  `Date` datetime DEFAULT NULL,
  `commentText` varchar(100) NOT NULL,
  `postID` int(11) NOT NULL,
  `postEmail` varchar(50) NOT NULL DEFAULT '',
  PRIMARY KEY (`ID`,`Email`),
  KEY `FK_comments_posts` (`postID`,`postEmail`),
  CONSTRAINT `FK_comments_posts` FOREIGN KEY (`postID`, `postEmail`) REFERENCES `posts` (`ID`, `Email`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Dumpar data för tabell gnu.comments: ~0 rows (ungefär)
/*!40000 ALTER TABLE `comments` DISABLE KEYS */;
/*!40000 ALTER TABLE `comments` ENABLE KEYS */;

-- Dumpar struktur för tabell gnu.discussions
CREATE TABLE IF NOT EXISTS `discussions` (
  `ID` int(11) NOT NULL,
  `Email` varchar(50) NOT NULL DEFAULT '',
  `Headline` varchar(50) DEFAULT NULL,
  `discussionText` varchar(500) DEFAULT NULL,
  `userName` varchar(50) DEFAULT NULL,
  `Date` datetime DEFAULT NULL,
  `tagOne` int(11) DEFAULT NULL,
  `tagTwo` int(11) DEFAULT NULL,
  `tagThree` int(11) DEFAULT NULL,
  PRIMARY KEY (`ID`,`Email`),
  KEY `FK_discussions_tags` (`tagOne`),
  KEY `FK_discussions_tags_2` (`tagTwo`),
  KEY `FK_discussions_tags_3` (`tagThree`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Dumpar data för tabell gnu.discussions: ~0 rows (ungefär)
/*!40000 ALTER TABLE `discussions` DISABLE KEYS */;
/*!40000 ALTER TABLE `discussions` ENABLE KEYS */;

-- Dumpar struktur för tabell gnu.messages
CREATE TABLE IF NOT EXISTS `messages` (
  `ID` int(11) NOT NULL,
  `Sent` datetime DEFAULT NULL,
  `From` varchar(50) DEFAULT NULL,
  `To` varchar(50) DEFAULT NULL,
  `messageText` varchar(200) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Dumpar data för tabell gnu.messages: ~0 rows (ungefär)
/*!40000 ALTER TABLE `messages` DISABLE KEYS */;
/*!40000 ALTER TABLE `messages` ENABLE KEYS */;

-- Dumpar struktur för tabell gnu.myfriends
CREATE TABLE IF NOT EXISTS `myfriends` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `userName` varchar(50) DEFAULT NULL,
  `Email` varchar(50) NOT NULL,
  `IsFriend` bit(1) DEFAULT NULL,
  `pubKey` text DEFAULT NULL,
  `userInfo` varchar(300) DEFAULT NULL,
  `pictureID` int(11) DEFAULT NULL,
  `tagOne` int(11) DEFAULT NULL,
  `tagTwo` int(11) DEFAULT NULL,
  `tagThree` int(11) DEFAULT NULL,
  `hideMe` bit(1) DEFAULT NULL,
  `hideFriend` bit(1) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `Email` (`Email`)
) ENGINE=InnoDB AUTO_INCREMENT=121 DEFAULT CHARSET=latin1;

-- Dumpar data för tabell gnu.myfriends: ~0 rows (ungefär)
/*!40000 ALTER TABLE `myfriends` DISABLE KEYS */;
/*!40000 ALTER TABLE `myfriends` ENABLE KEYS */;

-- Dumpar struktur för tabell gnu.myfriendsfriends
CREATE TABLE IF NOT EXISTS `myfriendsfriends` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `myFriendEmail` varchar(50) NOT NULL,
  `userName` varchar(50) DEFAULT NULL,
  `Email` varchar(50) DEFAULT NULL,
  `pictureID` int(11) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `FK_myfriendsfriends_myfriends` (`myFriendEmail`),
  CONSTRAINT `FK_myfriendsfriends_myfriends` FOREIGN KEY (`myFriendEmail`) REFERENCES `myfriends` (`Email`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=53 DEFAULT CHARSET=latin1;

-- Dumpar data för tabell gnu.myfriendsfriends: ~0 rows (ungefär)
/*!40000 ALTER TABLE `myfriendsfriends` DISABLE KEYS */;
/*!40000 ALTER TABLE `myfriendsfriends` ENABLE KEYS */;

-- Dumpar struktur för tabell gnu.myprofile
CREATE TABLE IF NOT EXISTS `myprofile` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Email` varchar(50) DEFAULT NULL,
  `myUserInfo` varchar(50) DEFAULT NULL,
  `pictureID` int(11) DEFAULT NULL,
  `tagOne` int(11) DEFAULT NULL,
  `tagTwo` int(11) DEFAULT NULL,
  `tagThree` int(11) DEFAULT NULL,
  `MyPubKey` text DEFAULT NULL,	
  PRIMARY KEY (`ID`),
  KEY `FK_myprofile_tags` (`tagOne`),
  KEY `FK_myprofile_tags_2` (`tagTwo`),
  KEY `FK_myprofile_tags_3` (`tagThree`),
  KEY `FK_myprofile_standardpictures` (`pictureID`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=latin1;

-- Dumpar data för tabell gnu.myprofile: ~1 rows (ungefär)
/*!40000 ALTER TABLE `myprofile` DISABLE KEYS */;
/*!40000 ALTER TABLE `myprofile` ENABLE KEYS */;

-- Dumpar struktur för tabell gnu.mysettings
CREATE TABLE IF NOT EXISTS `mysettings` (
  `ID` int(11) DEFAULT NULL,
  `Email` varchar(75) DEFAULT NULL,
  `Password` varchar(75) DEFAULT NULL,
  `userName` varchar(75) DEFAULT NULL,
  `Secret` varchar(75) DEFAULT NULL,
  `DarkMode` bit(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Dumpar data för tabell gnu.mysettings: ~0 rows (ungefär)
/*!40000 ALTER TABLE `mysettings` DISABLE KEYS */;
/*!40000 ALTER TABLE `mysettings` ENABLE KEYS */;

-- Dumpar struktur för tabell gnu.notifications
CREATE TABLE IF NOT EXISTS `notifications` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `hasBeenRead` bit(1) NOT NULL DEFAULT b'0',
  `messageType` varchar(50) NOT NULL DEFAULT '0',
  `info` varchar(50) DEFAULT '',
  `mail` varchar(50) NOT NULL DEFAULT '',
  `counter` int(11) NOT NULL DEFAULT 0,
  `infoID` int(11) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=latin1;

-- Dumpar data för tabell gnu.notifications: ~0 rows (ungefär)
/*!40000 ALTER TABLE `notifications` DISABLE KEYS */;
/*!40000 ALTER TABLE `notifications` ENABLE KEYS */;

-- Dumpar struktur för tabell gnu.posts
CREATE TABLE IF NOT EXISTS `posts` (
  `ID` int(11) NOT NULL,
  `Email` varchar(50) NOT NULL,
  `userName` varchar(50) NOT NULL DEFAULT '',
  `postText` varchar(1000) NOT NULL DEFAULT '',
  `Date` datetime NOT NULL,
  `discussionID` int(11) NOT NULL,
  `discussionEmail` varchar(50) NOT NULL,
  PRIMARY KEY (`ID`,`Email`),
  KEY `FK_posts_discussions` (`discussionID`,`discussionEmail`),
  CONSTRAINT `FK_posts_discussions` FOREIGN KEY (`discussionID`, `discussionEmail`) REFERENCES `discussions` (`ID`, `Email`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Dumpar data för tabell gnu.posts: ~0 rows (ungefär)
/*!40000 ALTER TABLE `posts` DISABLE KEYS */;
/*!40000 ALTER TABLE `posts` ENABLE KEYS */;

-- Dumpar struktur för tabell gnu.standardpictures
CREATE TABLE IF NOT EXISTS `standardpictures` (
  `pictureID` int(11) NOT NULL AUTO_INCREMENT,
  `PictureName` varchar(50) DEFAULT NULL,
  `PictureSrc` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`pictureID`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1;

-- Dumpar data för tabell gnu.standardpictures: ~5 rows (ungefär)
/*!40000 ALTER TABLE `standardpictures` DISABLE KEYS */;
INSERT INTO `standardpictures` (`pictureID`, `PictureName`, `PictureSrc`) VALUES
	(1, 'BeerGuy', '/image/BeerGuy.jpg'),
	(2, 'Flanders', '/image/Flanders.png'),
	(3, 'Nelson', '/image/Nelson.jpg'),
	(4, 'Ralph', '/image/Ralph.jpg'),
	(5, 'SideShow-Bob', '~/image/SideShow-Bob.jpg');
/*!40000 ALTER TABLE `standardpictures` ENABLE KEYS */;

-- Dumpar struktur för tabell gnu.tags
CREATE TABLE IF NOT EXISTS `tags` (
  `ID` int(11) NOT NULL,
  `tagName` varchar(50) NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Dumpar data för tabell gnu.tags: ~20 rows (ungefär)
/*!40000 ALTER TABLE `tags` DISABLE KEYS */;
INSERT INTO `tags` (`ID`, `tagName`) VALUES
	(1, 'Spel'),
	(2, 'Katter'),
	(3, 'Mat'),
	(4, 'Djur'),
	(5, 'Sport'),
	(6, 'Livstil'),
	(7, 'Datorer'),
	(8, 'Foto'),
	(9, 'Musik'),
	(10, 'Film'),
	(11, 'Böcker'),
	(12, 'Dejting'),
	(13, 'Resa'),
	(14, 'Väder'),
	(15, 'Kläder'),
	(16, 'Software'),
	(17, 'Utbildning'),
	(18, 'Porr'),
	(19, 'Fordon'),
	(20, 'Pengar');
/*!40000 ALTER TABLE `tags` ENABLE KEYS */;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;";
    }
}
