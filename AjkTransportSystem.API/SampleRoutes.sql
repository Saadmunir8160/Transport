-- AJK Transport System - Sample Routes
-- Run this script in MySQL to add routes for all cities

USE ajktransportdb;

-- Clear existing routes (optional)
-- DELETE FROM Routes;

-- Insert Routes
INSERT INTO Routes (RouteName, StartPoint, EndPoint, Distance) VALUES
-- Muzaffarabad Routes
('Muzaffarabad Express', 'Muzaffarabad', 'Rawalpindi', 138),
('Muzaffarabad Metro', 'Muzaffarabad', 'Islamabad', 150),
('Muzaffarabad-Mirpur Link', 'Muzaffarabad', 'Mirpur', 95),
('Muzaffarabad-Bagh Route', 'Muzaffarabad', 'Bagh', 80),
('Muzaffarabad-Neelum Valley', 'Muzaffarabad', 'Neelum', 105),

-- Mirpur Routes
('Mirpur Express', 'Mirpur', 'Rawalpindi', 120),
('Mirpur Metro', 'Mirpur', 'Islamabad', 140),
('Mirpur-Kotli Link', 'Mirpur', 'Kotli', 35),
('Mirpur-Bhimber Route', 'Mirpur', 'Bhimber', 45),

-- Rawalakot Routes
('Rawalakot Express', 'Rawalakot', 'Rawalpindi', 160),
('Rawalakot Metro', 'Rawalakot', 'Islamabad', 175),
('Rawalakot-Bagh Link', 'Rawalakot', 'Bagh', 55),

-- Kotli Routes
('Kotli Express', 'Kotli', 'Rawalpindi', 110),
('Kotli Metro', 'Kotli', 'Islamabad', 130),
('Kotli-Mirpur Link', 'Kotli', 'Mirpur', 35),

-- Bagh Routes
('Bagh Express', 'Bagh', 'Rawalpindi', 145),
('Bagh Metro', 'Bagh', 'Islamabad', 160),
('Bagh-Muzaffarabad Link', 'Bagh', 'Muzaffarabad', 80),

-- Bhimber Routes
('Bhimber Express', 'Bhimber', 'Rawalpindi', 95),
('Bhimber Metro', 'Bhimber', 'Islamabad', 115),
('Bhimber-Mirpur Link', 'Bhimber', 'Mirpur', 45),

-- Neelum Routes
('Neelum Valley Express', 'Neelum', 'Muzaffarabad', 105),
('Neelum-Rawalpindi Route', 'Neelum', 'Rawalpindi', 240),

-- Hattian Routes
('Hattian Express', 'Hattian', 'Rawalpindi', 155),
('Hattian-Muzaffarabad Link', 'Hattian', 'Muzaffarabad', 40),

-- Haveli Routes
('Haveli Express', 'Haveli', 'Rawalpindi', 125),
('Haveli-Kotli Link', 'Haveli', 'Kotli', 30),

-- Sudhnoti Routes
('Sudhnoti Express', 'Sudhnoti', 'Rawalpindi', 135),
('Sudhnoti-Rawalakot Link', 'Sudhnoti', 'Rawalakot', 25),

-- Rawalpindi to AJK Routes
('Rawalpindi-Muzaffarabad', 'Rawalpindi', 'Muzaffarabad', 138),
('Rawalpindi-Mirpur', 'Rawalpindi', 'Mirpur', 120),
('Rawalpindi-Kotli', 'Rawalpindi', 'Kotli', 110),

-- Islamabad to AJK Routes
('Islamabad-Muzaffarabad', 'Islamabad', 'Muzaffarabad', 150),
('Islamabad-Mirpur', 'Islamabad', 'Mirpur', 140),
('Islamabad-Rawalakot', 'Islamabad', 'Rawalakot', 175);

-- Verify inserted routes
SELECT COUNT(*) as TotalRoutes FROM Routes;
SELECT * FROM Routes ORDER BY RouteId DESC LIMIT 10;
