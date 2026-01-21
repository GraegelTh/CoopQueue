# CoopQueue

**Collaborative Game Backlog & Voting System**

CoopQueue is a full-stack web application designed to democratize choosing the next co-op game. It allows gaming groups to search for titles via IGDB, suggest additions to a shared queue, and democratically vote on what to play next.

<div align="center">
  <img src="https://github.com/user-attachments/assets/c182798c-149b-4b1e-9517-791ebfff6fce" width="100%" alt="CoopQueue Demo Animation">
</div>
<br>

*(Manage your backlog and vote for the next adventure)*




## Project Context
Built as an **Apprenticeship Capstone Project**.
The primary goal was to demonstrate full-stack development skills, focusing on **Clean Architecture**, **Container Orchestration**, and **Custom Security Implementations** without relying on pre-built identity frameworks.

---

## Learning Objectives

* **Architectural Patterns:** Implementing clean separation of concerns using DTOs to decouple database entities.
* **API Integration:** Practical consumption and robust mapping of third-party REST APIs (IGDB).
* **Security Mechanics:** Understanding the "black box" of auth by manually implementing JWT issuance and password salting.
* **Modern UX:** Implementing **Optimistic UI Updates** in Blazor for instant feedback.
* **DevOps:** Managing the full application lifecycle via Docker Compose.

---

## Tech Stack

* **.NET 8** – ASP.NET Core Web API + Blazor WebAssembly (WASM)
* **MudBlazor** – Material Design UI Library with Dark Mode support
* **SQL Server 2022** – Relational Database Management
* **Docker & Docker Compose** – Full container orchestration (API + DB + Client)
* **Entity Framework Core** – ORM with Code-First approach & Auto-Migrations
* **IGDB Integration** – External REST API consumption

---

## Key Features

### Intelligent Game Search
Real-time integration with the **IGDB API**. The system automatically filters out noise (DLCs, skin packs) to ensure only main games and standalone expansions are suggested.

<img width="2559" height="1304" alt="Screenshot 2026-01-19 204351" src="https://github.com/user-attachments/assets/1d71dd96-d170-4566-acb6-130c2a5e5104" />


### Queue Management & Voting
* **Optimistic Updates:** The UI reflects voting actions immediately for a responsive user experience.
* **Status Workflow:** Organize games by *Suggestion*, *Playing*, *Completed*, or *Wait for Sale*.
* **Selection Modes:** Choose the next game democratically (most votes) or via a weighted lottery system.

### Custom Security Implementation
Instead of using ASP.NET Identity, a custom authentication system was built for educational purposes:
* **Manual Hashing:** Implemented password hashing and salting using `HMACSHA512`.
* **JWT Auth:** Custom issuance and validation of JSON Web Tokens.
* **RBAC:** Role-Based Access Control separates *User* and *Admin* privileges.

---

## Getting Started

This project is fully containerized. You can spin up the entire stack (Database + App) with a single command.

### Prerequisites
* [Docker Desktop](https://www.docker.com/products/docker-desktop)

### 1. Configure Secrets
Create a `.env` file in the root directory (same level as `docker-compose.yml`) and add your secrets. This ensures no sensitive data is hardcoded in the repository.

```env
# Database Configuration
DB_PASSWORD=YourStrong!Passw0rd

# IGDB API Keys (Get these from twitch.tv/developers)
IGDB_CLIENT_ID=your_client_id_here
IGDB_CLIENT_SECRET=your_client_secret_here

# Security
JWT_TOKEN=super_long_secret_key_at_least_64_chars_long
INVITE_CODE=GamingFriends2026 (Or whatever you would like it to be)
```

### 2. Run the Application
Open your terminal in the root folder and run:

```bash
docker-compose up --build
```
**That's it!**
* The **Database** will start and initialize automatically.
* The **App** will wait for the DB, apply migrations (`InitialCreate`), and start the web server.
* Access the app at: **http://localhost:8080**

*(Note: The app runs in Development mode inside the container to enable Swagger UI and Hot Reloading features).*

---

## Roadmap

Planned improvements to further harden and productionize the application:

### Completed
* [x] **Unit & Integration Tests:** Implemented basic **xUnit** tests using an **In-Memory Database** to validate critical business logic
* [x] **CI/CD Pipeline:** Set up a basic GitHub Action to automatically build the Docker image on every commit to ensure code stability.
* [x] **Container Orchestration:** Created a robust `docker-compose` setup to run SQL Server and the Web App with a single command.

### Future Goals
* [ ] **Live Updates:** Investigate SignalR integration to reflect votes in real-time across clients.
* [ ] **Test Coverage:** Expand unit tests to cover `UserService` and edge-cases in the voting logic.
* [ ] **Caching:** Optimize IGDB API usage by caching search results.
