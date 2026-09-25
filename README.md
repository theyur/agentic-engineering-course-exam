# Instructor-Student Visit Tracker

Exam project for practicing agentic software development and SDLC workflows.

## Purpose

A deliberately simplified application for instructors to track student attendance at recurring group classes such as yoga, dance, or music classes.

The application is intentionally small enough to implement during the course, while containing enough domain structure to exercise requirements analysis, architecture, domain modeling, implementation, testing, and review using agentic development workflows.

## Core Domain

- An instructor manages one group.
- A group contains students.
- Classes follow a recurring weekly schedule.
- The instructor records attendance for every scheduled class.
- Attendance history is retained for 3 months.
- Reports are available per student for:
  - 1 week
  - 1 month
  - 3 months

## Constraints

- One instructor belongs to exactly one group.
- No payments, subscriptions, billing, or accounting.
- Attendance data retention: 3 months.
- Data is stored in memory.
- The application seeds pseudo-random demo data on startup:
  - 3 instructors
  - 10 students per instructor/group
  - randomized names, age, and gender

## Technology

- C#
- .NET / ASP.NET Core
- Blazor
- Entity Framework Core
- EF Core InMemory provider

## Architecture

Clean Architecture:

`Domain ← Application ← Infrastructure`

The UI depends on the Application layer rather than directly on Infrastructure.

## Development

The project is developed primarily through an agentic SDLC workflow using Claude Code.
