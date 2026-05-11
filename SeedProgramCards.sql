-- Sample SQL script to populate ProgramCards table
-- Run this in your SQL Server database after the migration

-- Insert sample programs
INSERT INTO ProgramCards (ProgramTitle, ShortDescription, LongDescription, Link, ImageUrl, FlyerUrl, IsHybrid, DisplayIndex, IsActive)
VALUES 
(
    'Rising 6th Grade Avid Program',
    'Preparing students for a successful transition to middle school.',
    '# Rising 6th Grade AVID Program

## What is AVID?

**AVID (Advancement Via Individual Determination)** is a college readiness program designed to help students develop the skills they need to be successful in advanced classes and prepare for college and other postsecondary opportunities.

## Program Goals

- Develop strong organizational and study skills
- Build confidence in academic abilities
- Prepare for the transition to middle school
- Foster college and career readiness

## Who Should Participate?

This program is ideal for students who:

- Show academic potential
- Are motivated to challenge themselves
- Want to develop strong study habits
- Are preparing for middle school

## Program Activities

1. **Organizational Skills** - Learn binder organization and time management
2. **Note-Taking Strategies** - Cornell notes and other effective techniques
3. **Critical Thinking** - Develop analytical and problem-solving skills
4. **Study Groups** - Collaborative learning with peers

## Contact Information

For more information about the AVID program, please contact the PTA at [luxfordpta1@gmail.com](mailto:luxfordpta1@gmail.com).',
    '/programs/rising-6th-grade-avid',
    'images/programs/avid.png',
    'flyers/avid.pdf',
    0,
    1,
    1
),
(
    'Reflections',
    'A national arts recognition program for students.',
    '# PTA Reflections Arts Program

## About Reflections

Reflections is a **National PTA arts recognition program** that encourages students to explore their creativity through various art forms.

## 2025-2026 Theme

> "I am hopeful because..."

## Categories

Students can submit entries in the following categories:

- 🎨 **Visual Arts** - Drawing, painting, printmaking, collage
- 📸 **Photography** - Original photographs
- 📖 **Literature** - Poetry, prose, short stories
- 🎵 **Music Composition** - Original musical works
- 💃 **Dance Choreography** - Original dance performances
- 🎬 **Film Production** - Original video productions
- 🎭 **Special Artist** - For students with disabilities

## Important Dates

- **Submission Deadline**: October 31, 2025
- **School Awards**: November 15, 2025
- **District Competition**: December 2025
- **State Competition**: February 2026

## How to Participate

1. Choose your art category
2. Create your original artwork inspired by the theme
3. Complete the entry form
4. Submit by the deadline

## Rules & Guidelines

- All work must be original
- Students must complete their own work
- Entries must reflect the annual theme
- Follow specific guidelines for each category

For complete rules and entry forms, download the [Program Flyer](flyers/reflections.pdf).

## Awards

Students can win recognition at:
- School level
- District level
- State level
- **National level** - Outstanding entries advance!

## Questions?

Contact the Reflections coordinator through the PTA office at [luxfordpta1@gmail.com](mailto:luxfordpta1@gmail.com).',
    '/programs/reflections',
    'images/programs/reflections-25-26.png',
    'flyers/reflections.pdf',
    1,
    2,
    1
);

-- You can add more programs as needed
PRINT 'Sample ProgramCards inserted successfully!';
