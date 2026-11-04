-- Quick Test Data for Timings Management
-- Run this in phpMyAdmin to add routes and vehicles

USE ajktransportdb;

-- Add 5 Routes
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Muzaffarabad Express', 'Muzaffarabad', 'Rawalpindi', 138),
('Mirpur Express', 'Mirpur', 'Rawalpindi', 120),
('Kotli Metro', 'Kotli', 'Islamabad', 130),
('Bagh Express', 'Bagh', 'Rawalpindi', 145),
('Rawalakot Express', 'Rawalakot', 'Islamabad', 175);

-- Add 5 Vehicles
INSERT INTO Vehicles (VehicleNumber, VehicleType, Capacity, Status) VALUES
('AJK-1001', 'Coaster', 29, 'Active'),
('AJK-1002', 'Hiace', 15, 'Active'),
('AJK-1003', 'Coaster', 29, 'Active'),
('AJK-1004', 'Hiace', 15, 'Active'),
('AJK-1005', 'Coaster', 29, 'Active');

-- Verify
SELECT 'Routes:' as Info, COUNT(*) as Count FROM Routes
UNION ALL
SELECT 'Vehicles:' as Info, COUNT(*) as Count FROM Vehicles;

SELECT * FROM Routes;
SELECT * FROM Vehicles;
