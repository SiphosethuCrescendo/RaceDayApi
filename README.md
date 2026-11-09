# 🏃‍♂️ RaceDay - Event Management System

RaceDay is a full-stack web application designed for the South African road running, walking, and cycling community. The platform allows Event Organisers to create and manage events, while Participants can browse, enrol, and track their results.

## 👥 User Roles
- **Organiser:** Create, edit, and delete events, manage categories, view enrolments, and upload results.
- **Participant:** Browse upcoming events, enrol in categories, view personal enrolments, and track results.

---

## 📚 Part 1: System Planning & Database
- Designed the relational database using an Entity Relationship Diagram (ERD) with 7 entities.
- Created the full SQL script matching the ERD with constraints and sample data.
- Planned the RESTful API endpoints for all required functionality.

## ⚙️ Part 2: RESTful API Development
- Built an **ASP.NET Core Web API** connected to SQL Server using **Entity Framework Core (Code-First)**.
- Implemented **JWT Authentication** with role-based access control (Organiser vs. Participant).
- Passwords are securely hashed using **BCrypt**.
- Integrated **Swagger UI** for live endpoint testing.
- Wrote **27 xUnit Unit Tests** covering authentication, event management, and enrolment logic.

## 🖥️ Part 3: MVC Application & UI
- Built an **ASP.NET Core MVC** frontend that consumes the Part 2 API (no direct database access).
- Implemented session-based authentication to maintain user state across pages.
- Created distinct, role-based interfaces and navigation (Organiser Dashboard vs. Participant Portal).
- Used **Bootstrap 5** to create an energetic, sporting visual theme.
- Integrated **Azure Blob Storage** (Azurite emulator) to allow Organisers to upload event banner images.

- **Custom Authorization:** Implemented a custom `SessionAuthorizeAttribute` to protect MVC routes and redirect unauthenticated users to the login page.
- **Energetic UI Design:** Utilized Bootstrap 5 to create a modern, sporting aesthetic with custom gradients, card layouts, and responsive grids.
   
