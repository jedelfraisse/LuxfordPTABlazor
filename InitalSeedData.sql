-- =====================================================
-- Luxford PTA Event Management System - Initial Seed Data
-- Updated: September 15, 2025
-- Purpose: Initial setup for production deployment with refined PTA event categories
-- =====================================================

USE [LuxfordPTAWeb]
GO

-- =====================================================
-- 1. SCHOOL YEARS
-- =====================================================
SET IDENTITY_INSERT [SchoolYears] ON;

INSERT INTO [SchoolYears] ([Id], [Name], [StartDate], [EndDate], [PrintableEventCalendar])
VALUES 
    (1, '2023-2024', '2023-07-01', '2024-06-30', 'images/eventcal-2023-2024.pdf'),
    (2, '2024-2025', '2024-07-01', '2025-06-30', 'images/eventcal-2024-2025.pdf'),
    (3, '2025-2026', '2025-07-01', '2026-06-30', 'images/eventcal-2025-2026.jpg');

SET IDENTITY_INSERT [SchoolYears] OFF;

-- =====================================================
-- 2. EVENT Categories
-- =====================================================
SET IDENTITY_INSERT [EventCats] ON;

INSERT INTO [EventCats] ([Id], [Name], [Slug], [Description], [DisplayOrder], [IsActive], [Size], [Icon], [ColorClass], [DisplayMode], [MaxEventsToShow], [ShowViewEventsButton], [ShowInlineOnMainPage])
VALUES 
    (1, 'School Calendar & Closures', 'school-calendar-closures', 'Anchor dates everyone needs to know, such as holidays, staff days, and testing days.', 0, 1, 1, 'bi-calendar-x', 'text-danger', 0, 0, 1, 1),
    (2, 'Meetings & Planning', 'meetings-planning', 'PTA, committee, and board sessions for planning and coordination.', 1, 1, 0, 'bi-people-fill', 'text-primary', 0, 0, 1, 0),
    (3, 'Community Engagement', 'community-engagement', 'Events that build connection and fun for families and the school community.', 2, 1, 1, 'bi-heart-fill', 'text-warning', 0, 0, 1, 0),
    (4, 'Fundraising & Drives', 'fundraising-drives', 'Campaigns and donation efforts to support school programs and activities.', 3, 1, 1, 'bi-cash-coin', 'text-success', 0, 0, 1, 0),
    (5, 'Staff & School Support', 'staff-school-support', 'Events that help teachers and school operations, such as appreciation and supply drives.', 4, 1, 0, 'bi-mortarboard-fill', 'text-info', 0, 0, 1, 0),
    (6, 'Student/Family Support Projects', 'student-family-support', 'Ongoing initiatives with recurring events to support students and families.', 5, 1, 0, 'bi-hand-thumbs-up', 'text-secondary', 0, 0, 1, 0);

SET IDENTITY_INSERT [EventCats] OFF;

-- =====================================================
-- 3. EVENT SUB-TYPES (Subcategories)
-- =====================================================
SET IDENTITY_INSERT [EventCatSubs] ON;

INSERT INTO [EventCatSubs] ([Id], [Name], [Slug], [Description], [DisplayOrder], [IsActive], [Icon], [ColorClass], [EventCatId])
VALUES 
    -- School Calendar & Closures
    (1, 'Holiday', 'holiday', 'School holidays when school is closed for all students and staff.', 0, 1, 'bi-flag', 'text-danger', 1),
    (2, 'Staff Day', 'staff-day', 'Staff workdays or professional development days; no school for students.', 1, 1, 'bi-mortarboard', 'text-warning', 1),
    (3, 'Adjusted Dismissal', 'adjusted-dismissal', 'Days with early or late dismissal schedules.', 2, 1, 'bi-clock', 'text-info', 1),
    (4, 'Testing Day', 'testing-day', 'Standardized or school-wide testing days.', 3, 1, 'bi-clipboard-data', 'text-primary', 1),

    -- Meetings & Planning
    (5, 'Monthly PTA Meeting', 'monthly-pta', 'Regular monthly PTA meetings for all members.', 0, 1, 'bi-calendar-event', 'text-primary', 2),
    (6, 'Budget Planning', 'budget-planning', 'Sessions focused on PTA or school budget planning.', 1, 1, 'bi-cash-stack', 'text-info', 2),
    (7, 'Volunteer Coordination', 'volunteer-coordination', 'Meetings to organize and coordinate volunteers.', 2, 1, 'bi-people', 'text-success', 2),

    -- Community Engagement
    (8, 'Trunk or Treat', 'trunk-or-treat', 'A fun, safe Halloween event for families.', 0, 1, 'bi-car-front', 'text-warning', 3),
    (9, 'Family Game Night', 'family-game-night', 'Interactive game nights for families and students.', 1, 1, 'bi-dice-5', 'text-success', 3),
    (10, 'Talent Show', 'talent-show', 'Showcase of student and family talents.', 2, 1, 'bi-mic', 'text-info', 3),

    -- Fundraising & Drives
    (11, 'Candy Drive', 'candy-drive', 'Collecting candy for events or fundraising.', 0, 1, 'bi-bag', 'text-success', 4),
    (12, 'Sponsor Campaign', 'sponsor-campaign', 'Campaigns to secure business or community sponsors.', 1, 1, 'bi-megaphone', 'text-warning', 4),
    (13, 'Spirit Wear Sale', 'spirit-wear-sale', 'Sales of school-branded clothing and items.', 2, 1, 'bi-shop', 'text-primary', 4),

    -- Staff & School Support
    (14, 'Teacher Appreciation', 'teacher-appreciation', 'Events to recognize and thank teachers.', 0, 1, 'bi-award', 'text-info', 5),
    (15, 'Supply Drive', 'supply-drive', 'Drives to collect classroom or school supplies.', 1, 1, 'bi-box-seam', 'text-success', 5),
    (16, 'Beautification Day', 'beautification-day', 'School clean-up or improvement projects.', 2, 1, 'bi-brush', 'text-warning', 5),

    -- Student/Family Support Projects
    (17, 'Garden Project', 'garden-project', 'Ongoing school or community garden initiatives.', 0, 1, 'bi-flower1', 'text-success', 6),
    (18, 'Safety Campaign', 'safety-campaign', 'Programs to promote student and family safety.', 1, 1, 'bi-shield-check', 'text-danger', 6),
    (19, 'Wellness Workshop', 'wellness-workshop', 'Workshops focused on health and wellness for families.', 2, 1, 'bi-heart-pulse', 'text-info', 6);

SET IDENTITY_INSERT [EventCatSubs] OFF;

-- =====================================================
-- 4. SPONSORS (Sample Data)
-- =====================================================
SET IDENTITY_INSERT [Sponsors] ON;

INSERT INTO [Sponsors] ([Id], [Name], [LogoUrl], [WebsiteUrl])
VALUES 
    (1, 'Chuck E. Cheese''s', 'sponsors/chuck-e-cheeses-car-design-png-logo-8.png', 'https://www.chuckecheese.com/'),
    (2, 'Local Pizza Company', 'sponsors/sample-sponsor-logo.png', 'https://example.com/pizza'),
    (3, 'Community Bank', 'sponsors/community-bank-logo.png', 'https://example.com/bank'),
    (4, 'Local Bookstore', 'sponsors/bookstore-logo.png', 'https://example.com/books'),
    (5, 'Hardware Store', 'sponsors/hardware-logo.png', 'https://example.com/hardware');

SET IDENTITY_INSERT [Sponsors] OFF;

-- =====================================================
-- END OF SEED DATA SCRIPT
-- =====================================================

PRINT 'Luxford PTA Event Management System seed data has been successfully inserted!';
PRINT 'Remember to:';
PRINT '- Set up admin users through the application';
PRINT '- Assign coordinator roles to events';
PRINT '- Add sponsor logo files to wwwroot/sponsors/';
PRINT '- Update calendar PDF/image paths as needed';
