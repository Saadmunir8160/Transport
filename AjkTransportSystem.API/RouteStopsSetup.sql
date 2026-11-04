-- Create RouteStops Table
CREATE TABLE IF NOT EXISTS RouteStops (
    RouteStopId INT AUTO_INCREMENT PRIMARY KEY,
    RouteId INT NOT NULL,
    StopName VARCHAR(100) NOT NULL,
    StopOrder INT NOT NULL,
    DistanceFromStart INT NOT NULL,
    FareFromStart DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (RouteId) REFERENCES Routes(RouteId) ON DELETE CASCADE
);

-- Example: Bagh to Rawalpindi - 2 Different Routes

-- Route 1: Bagh → Murree → Rawalpindi (Via Murree)
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Bagh-Rawalpindi (Via Murree)', 'Bagh', 'Rawalpindi', 145);

SET @route1_id = LAST_INSERT_ID();

INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@route1_id, 'Bagh', 1, 0, 0),
(@route1_id, 'Sudhnoti', 2, 25, 300),
(@route1_id, 'Rawalakot', 3, 55, 660),
(@route1_id, 'Murree', 4, 95, 1140),
(@route1_id, 'Bhara Kahu', 5, 125, 1500),
(@route1_id, 'Rawalpindi', 6, 145, 1740);

-- Route 2: Bagh → Kotli → Sattian → Rawalpindi (Via Kotli)
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Bagh-Rawalpindi (Via Kotli)', 'Bagh', 'Rawalpindi', 150);

SET @route2_id = LAST_INSERT_ID();

INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@route2_id, 'Bagh', 1, 0, 0),
(@route2_id, 'Haveli', 2, 30, 360),
(@route2_id, 'Kotli', 3, 60, 720),
(@route2_id, 'Sattian', 4, 95, 1140),
(@route2_id, 'Islamabad', 5, 130, 1560),
(@route2_id, 'Rawalpindi', 6, 150, 1800);

-- More Example Routes with Stops

-- Muzaffarabad to Rawalpindi
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Muzaffarabad-Rawalpindi Express', 'Muzaffarabad', 'Rawalpindi', 138);

SET @route3_id = LAST_INSERT_ID();

INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@route3_id, 'Muzaffarabad', 1, 0, 0),
(@route3_id, 'Kohala', 2, 35, 420),
(@route3_id, 'Abbottabad', 3, 85, 1020),
(@route3_id, 'Haripur', 4, 115, 1380),
(@route3_id, 'Rawalpindi', 5, 138, 1656);

-- Mirpur to Rawalpindi
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Mirpur-Rawalpindi Direct', 'Mirpur', 'Rawalpindi', 120);

SET @route4_id = LAST_INSERT_ID();

INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@route4_id, 'Mirpur', 1, 0, 0),
(@route4_id, 'Bhimber', 2, 45, 540),
(@route4_id, 'Gujrat', 3, 80, 960),
(@route4_id, 'Jhelum', 4, 100, 1200),
(@route4_id, 'Rawalpindi', 5, 120, 1440);

-- Verify
SELECT 'Routes with Stops' as Info;
SELECT r.RouteName, rs.StopName, rs.StopOrder, rs.DistanceFromStart, rs.FareFromStart
FROM Routes r
JOIN RouteStops rs ON r.RouteId = rs.RouteId
ORDER BY r.RouteId, rs.StopOrder;
