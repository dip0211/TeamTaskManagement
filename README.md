# Team Task Management System

A full-stack, role-based task management and collaboration platform built with **ASP.NET Core 8 Web API**, **Entity Framework Core**, **SQL Server**, and **React (Vite + Tailwind CSS)**.

---

## 🚀 Key Features

- **Role-Based Access Control (RBAC)**:
  - **Admin**: Create and manage teams; assign tasks across all managers and users.
  - **Manager**: Create and assign deliverables strictly to members of their assigned team.
  - **User**: View, track, and update the status of assigned tasks.
- **Authentication & Security**:
  - Secure JWT authentication with automated refresh token rotation.
  - Axios 401 response interceptor for session renewal.
  - Cryptographic password hashing using PBKDF2 (`Rfc2898DeriveBytes`).
- **Core Workflow & Collaboration**:
  - Task status tracking: `To Do`, `In Progress`, and `Done`.
  - Task priority management: `Low`, `Medium`, and `High`.
  - Granular task dashboard filtering by status, priority, and deadline.
  - Task discussion and commenting threads.
- **Event-Driven Notifications**:
  - Mock notification service triggers on task assignments and status transitions.
- **Bonus Implementations**:
  - **Multi-Container Docker**: Single command deployment via `docker-compose.yml`.
  - **CI/CD Pipeline**: GitHub Actions for automated backend building, testing, and frontend verification.
  - **Interactive API Documentation**: Swagger UI (OpenAPI) and bundled Postman Collection.
  - **Automated Tests**: Comprehensive xUnit, Moq, and FluentAssertions unit test suite.

---

## 🛠️ Technology Stack

- **Backend**: .NET 8 (C#), ASP.NET Core Web API, EF Core 8.
- **Database**: Microsoft SQL Server.
- **Frontend**: React 18, Vite, Tailwind CSS v4, Lucide Icons, Axios.
- **Testing**: xUnit, Moq, FluentAssertions, EF Core In-Memory.
- **DevOps**: Docker, Docker Compose, Nginx, GitHub Actions.

---

## ⚙️ Quick Start Guide

### Option 1: Run with Docker Compose (Recommended)

Make sure **Docker Desktop** is running, then execute:

```bash
docker compose up --build