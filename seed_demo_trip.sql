-- Add Muzaffarabad to Rawalpindi Route, Fare, and Scheduled Trip for FYP Demo
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance, Status, BaseFare, BaselineDuration, IsSeasonal) 
VALUES ('Muzaffarabad - Rawalpindi Express', 'Muzaffarabad', 'Rawalpindi', 138.0, 'Active', 650.0, 210, 0);

DECLARE @rId INT = SCOPE_IDENTITY();

INSERT INTO Fares (RouteId, Amount, PassengerType) 
VALUES (@rId, 650.0, 'Adult');

INSERT INTO Trips (RouteId, VehicleId, DriverId, TravelDate, DepartureTime, Status, BookedSeats, DelayInMinutes) 
VALUES (@rId, 1, 5, DATEADD(day, 1, CAST(GETDATE() AS DATE)), '09:00:00', 'Scheduled', 0, 0);
