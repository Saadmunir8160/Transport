-- ========================================
-- AJK TRANSPORT SYSTEM - COMPLETE NETWORK
-- All Cities with Multiple Routes & Stops
-- ========================================

USE ajktransportdb;

-- Clear existing data (optional)
-- DELETE FROM RouteStops;
-- DELETE FROM Routes WHERE RouteId > 0;

-- ========================================
-- BAGH ROUTES (2 paths to Rawalpindi)
-- ========================================

-- Route 1: Bagh → Murree → Rawalpindi (Mountain Route)
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Bagh-Rawalpindi (Via Murree)', 'Bagh', 'Rawalpindi', 145);
SET @r1 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r1, 'Bagh', 1, 0, 100),           -- Base fare Rs. 100
(@r1, 'Sudhnoti', 2, 25, 400),      -- +300
(@r1, 'Rawalakot', 3, 55, 760),     -- +360
(@r1, 'Murree', 4, 95, 1240),       -- +480
(@r1, 'Bhara Kahu', 5, 125, 1600),  -- +360
(@r1, 'Rawalpindi', 6, 145, 1840);  -- +240 (Direct: Rs. 1840)

-- Route 2: Bagh → Kotli → Rawalpindi (Valley Route)
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Bagh-Rawalpindi (Via Kotli)', 'Bagh', 'Rawalpindi', 150);
SET @r2 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r2, 'Bagh', 1, 0, 100),
(@r2, 'Haveli', 2, 30, 460),
(@r2, 'Kotli', 3, 60, 820),
(@r2, 'Mirpur', 4, 95, 1240),
(@r2, 'Islamabad', 5, 130, 1660),
(@r2, 'Rawalpindi', 6, 150, 1900);  -- Direct: Rs. 1900

-- ========================================
-- MUZAFFARABAD ROUTES
-- ========================================

-- Muzaffarabad → Rawalpindi (Main Highway)
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Muzaffarabad-Rawalpindi Express', 'Muzaffarabad', 'Rawalpindi', 138);
SET @r3 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r3, 'Muzaffarabad', 1, 0, 100),
(@r3, 'Kohala', 2, 35, 520),
(@r3, 'Abbottabad', 3, 85, 1120),
(@r3, 'Haripur', 4, 115, 1480),
(@r3, 'Rawalpindi', 5, 138, 1756);  -- Direct: Rs. 1756

-- Muzaffarabad → Islamabad
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Muzaffarabad-Islamabad Metro', 'Muzaffarabad', 'Islamabad', 150);
SET @r4 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r4, 'Muzaffarabad', 1, 0, 100),
(@r4, 'Kohala', 2, 35, 520),
(@r4, 'Abbottabad', 3, 85, 1120),
(@r4, 'Islamabad', 4, 150, 1900);   -- Direct: Rs. 1900

-- Muzaffarabad → Neelum Valley
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Muzaffarabad-Neelum Valley', 'Muzaffarabad', 'Neelum', 105);
SET @r5 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r5, 'Muzaffarabad', 1, 0, 100),
(@r5, 'Kundal Shahi', 2, 35, 520),
(@r5, 'Athmuqam', 3, 70, 940),
(@r5, 'Neelum', 4, 105, 1360);      -- Direct: Rs. 1360

-- ========================================
-- MIRPUR ROUTES
-- ========================================

-- Mirpur → Rawalpindi (Direct)
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Mirpur-Rawalpindi Direct', 'Mirpur', 'Rawalpindi', 120);
SET @r6 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r6, 'Mirpur', 1, 0, 100),
(@r6, 'Bhimber', 2, 45, 640),
(@r6, 'Gujrat', 3, 80, 1060),
(@r6, 'Jhelum', 4, 100, 1300),
(@r6, 'Rawalpindi', 5, 120, 1540);  -- Direct: Rs. 1540

-- Mirpur → Islamabad
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Mirpur-Islamabad Metro', 'Mirpur', 'Islamabad', 140);
SET @r7 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r7, 'Mirpur', 1, 0, 100),
(@r7, 'Bhimber', 2, 45, 640),
(@r7, 'Gujrat', 3, 80, 1060),
(@r7, 'Islamabad', 4, 140, 1780);   -- Direct: Rs. 1780

-- Mirpur → Kotli
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Mirpur-Kotli Link', 'Mirpur', 'Kotli', 35);
SET @r8 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r8, 'Mirpur', 1, 0, 100),
(@r8, 'Dadyal', 2, 18, 316),
(@r8, 'Kotli', 3, 35, 520);         -- Direct: Rs. 520

-- ========================================
-- RAWALAKOT ROUTES
-- ========================================

-- Rawalakot → Rawalpindi
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Rawalakot-Rawalpindi Express', 'Rawalakot', 'Rawalpindi', 160);
SET @r9 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r9, 'Rawalakot', 1, 0, 100),
(@r9, 'Bagh', 2, 30, 460),
(@r9, 'Murree', 3, 70, 940),
(@r9, 'Rawalpindi', 4, 160, 2020);  -- Direct: Rs. 2020

-- Rawalakot → Islamabad
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Rawalakot-Islamabad Metro', 'Rawalakot', 'Islamabad', 175);
SET @r10 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r10, 'Rawalakot', 1, 0, 100),
(@r10, 'Bagh', 2, 30, 460),
(@r10, 'Murree', 3, 70, 940),
(@r10, 'Islamabad', 4, 175, 2200);  -- Direct: Rs. 2200

-- ========================================
-- KOTLI ROUTES
-- ========================================

-- Kotli → Rawalpindi
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Kotli-Rawalpindi Express', 'Kotli', 'Rawalpindi', 110);
SET @r11 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r11, 'Kotli', 1, 0, 100),
(@r11, 'Mirpur', 2, 35, 520),
(@r11, 'Jhelum', 3, 75, 1000),
(@r11, 'Rawalpindi', 4, 110, 1420);  -- Direct: Rs. 1420

-- Kotli → Islamabad
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Kotli-Islamabad Metro', 'Kotli', 'Islamabad', 130);
SET @r12 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r12, 'Kotli', 1, 0, 100),
(@r12, 'Mirpur', 2, 35, 520),
(@r12, 'Islamabad', 3, 130, 1660);  -- Direct: Rs. 1660

-- ========================================
-- BHIMBER ROUTES
-- ========================================

-- Bhimber → Rawalpindi
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Bhimber-Rawalpindi Express', 'Bhimber', 'Rawalpindi', 95);
SET @r13 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r13, 'Bhimber', 1, 0, 100),
(@r13, 'Gujrat', 2, 40, 580),
(@r13, 'Jhelum', 3, 65, 880),
(@r13, 'Rawalpindi', 4, 95, 1240);  -- Direct: Rs. 1240

-- Bhimber → Islamabad
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Bhimber-Islamabad Metro', 'Bhimber', 'Islamabad', 115);
SET @r14 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r14, 'Bhimber', 1, 0, 100),
(@r14, 'Gujrat', 2, 40, 580),
(@r14, 'Islamabad', 3, 115, 1480);  -- Direct: Rs. 1480

-- ========================================
-- NEELUM VALLEY ROUTES
-- ========================================

-- Neelum → Muzaffarabad
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Neelum-Muzaffarabad Valley', 'Neelum', 'Muzaffarabad', 105);
SET @r15 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r15, 'Neelum', 1, 0, 100),
(@r15, 'Athmuqam', 2, 35, 520),
(@r15, 'Kundal Shahi', 3, 70, 940),
(@r15, 'Muzaffarabad', 4, 105, 1360);  -- Direct: Rs. 1360

-- ========================================
-- HATTIAN ROUTES
-- ========================================

-- Hattian → Rawalpindi
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Hattian-Rawalpindi Express', 'Hattian', 'Rawalpindi', 155);
SET @r16 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r16, 'Hattian', 1, 0, 100),
(@r16, 'Muzaffarabad', 2, 40, 580),
(@r16, 'Abbottabad', 3, 95, 1240),
(@r16, 'Rawalpindi', 4, 155, 1960);  -- Direct: Rs. 1960

-- ========================================
-- HAVELI ROUTES
-- ========================================

-- Haveli → Rawalpindi
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Haveli-Rawalpindi Express', 'Haveli', 'Rawalpindi', 125);
SET @r17 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r17, 'Haveli', 1, 0, 100),
(@r17, 'Kotli', 2, 30, 460),
(@r17, 'Mirpur', 3, 65, 880),
(@r17, 'Rawalpindi', 4, 125, 1600);  -- Direct: Rs. 1600

-- ========================================
-- SUDHNOTI ROUTES
-- ========================================

-- Sudhnoti → Rawalpindi
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Sudhnoti-Rawalpindi Express', 'Sudhnoti', 'Rawalpindi', 135);
SET @r18 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r18, 'Sudhnoti', 1, 0, 100),
(@r18, 'Rawalakot', 2, 25, 400),
(@r18, 'Bagh', 3, 50, 700),
(@r18, 'Rawalpindi', 4, 135, 1720);  -- Direct: Rs. 1720

-- ========================================
-- REVERSE ROUTES (Rawalpindi/Islamabad to AJK)
-- ========================================

-- Rawalpindi → Muzaffarabad
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Rawalpindi-Muzaffarabad', 'Rawalpindi', 'Muzaffarabad', 138);
SET @r19 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r19, 'Rawalpindi', 1, 0, 100),
(@r19, 'Haripur', 2, 23, 376),
(@r19, 'Abbottabad', 3, 53, 736),
(@r19, 'Kohala', 4, 103, 1336),
(@r19, 'Muzaffarabad', 5, 138, 1756);

-- Islamabad → Muzaffarabad
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
('Islamabad-Muzaffarabad', 'Islamabad', 'Muzaffarabad', 150);
SET @r20 = LAST_INSERT_ID();
INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart) VALUES
(@r20, 'Islamabad', 1, 0, 100),
(@r20, 'Abbottabad', 2, 65, 880),
(@r20, 'Kohala', 3, 115, 1480),
(@r20, 'Muzaffarabad', 4, 150, 1900);

-- ========================================
-- VERIFICATION QUERY
-- ========================================
SELECT 
    r.RouteName,
    r.StartPoint,
    r.EndPoint,
    COUNT(rs.RouteStopId) as TotalStops,
    MAX(rs.FareFromStart) as DirectFare
FROM Routes r
LEFT JOIN RouteStops rs ON r.RouteId = rs.RouteId
GROUP BY r.RouteId
ORDER BY r.StartPoint, r.EndPoint;

-- Show all stops for a specific route
SELECT 
    r.RouteName,
    rs.StopOrder,
    rs.StopName,
    rs.DistanceFromStart as 'Distance (km)',
    rs.FareFromStart as 'Fare (Rs.)'
FROM Routes r
JOIN RouteStops rs ON r.RouteId = rs.RouteId
WHERE r.RouteName LIKE '%Bagh%'
ORDER BY r.RouteId, rs.StopOrder;
