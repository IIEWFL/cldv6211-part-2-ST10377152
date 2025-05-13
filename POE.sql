--DATABASE CREATION SECTION
use master
IF EXISTS (SELECT * FROM  sys.databases WHERE name = 'POEDB')
DROP DATABASE POEDB
CREATE DATABASE POEDB
use POEDB

CREATE TABLE Venue (
VenueID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
VenueName VARCHAR(20) UNIQUE NOT NULL,
[Location] VARCHAR(20)  NOT NULL,
Capacity INT NOT NULL,
ImageUrl VARCHAR(MAX)
);

CREATE TABLE [Event] (
EventID INT  IDENTITY (1,1) PRIMARY KEY NOT NULL,
EventName VARCHAR(20) NOT NULL,
EventDate VARCHAR(20) NOT NULL,
[Description] VARCHAR(250) NOT NULL,
);

CREATE TABLE Booking (
BookingID INT IDENTITY (1,1) PRIMARY  KEY NOT NULL,
VenueID INT FOREIGN KEY REFERENCES Venue(VenueID) NOT NULL,
EventID INT FOREIGN KEY REFERENCES [Event](EventID) NOT NULL,
BookingDate DATE NOT NULL
);

--TABLE ALTERATION SECTION

--TABLE INSERTION SECTION
INSERT INTO Venue (VenueName, [Location], Capacity, ImageUrl)
VALUES ('Queen', 'Newcastle', '70', 'https://images.aeonmedia.co/images/acd6897d-9849-4188-92c6-79dabcbcd518/essay-final-gettyimages-685469924.jpg?width=3840&quality=75&format=auto'),
('Kingland', 'Liverpool', '150', ''),
('Jacksland', 'Manchester', '300','https://hips.hearstapps.com/hmg-prod/images/livestock-dogs-farm-dogs-german-shepherd-66e8667aed873.jpg?crop=0.6669811320754717xw:1xh;center,top&resize=980:*')

INSERT INTO [Event](EventName, EventDate, [Description])
VALUES ('Party night', '2025/01/28', 'Seventh year party'),
('Defeat in main stage', '2025/02/03', 'Lost Embrasserly'),
('The number one', '2025/04/15', 'This is the one'),
('The paid one', '2025/05/21', 'Paid at the end')

INSERT INTO Booking(VenueID, EventID, BookingDate)
VALUES (1, 1, '2025/01/25'),
(2, 2, '2025/02/01')

--TABLE MANIPULATION SECTION 

SELECT * FROM Venue
SELECT * FROM [Event]
SELECT * FROM Booking

SELECT Booking.BookingID, Venue.VenueName, [Event].EventName, BookingDate
FROM Booking
JOIN Venue ON Booking.VenueID = Venue.VenueID
JOIN [Event] ON Booking.EventID = [Event].EventID;
GO

CREATE VIEW BookingView AS
SELECT Booking.BookingID, Venue.VenueName, [Event].EventName, BookingDate
FROM Booking
JOIN Venue ON Booking.VenueID = Venue.VenueID
JOIN [Event] ON Booking.EventID = [Event].EventID
;
GO


SELECT * FROM BookingView

--STORED PROCEDURES SECTION
