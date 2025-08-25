# BoardForge – API (JWT, RBAC, Files, Azure)

**BoardForge** is a sample Kanban-style task management API built with **.NET 8**.  
It’s designed as a **teaching project** to demonstrate modern API practices:

- JWT authentication & refresh tokens
- Claims-based **RBAC (Role-Based Access Control)** per team
- Many-to-many relationships and query specification pattern
- Optimistic concurrency using SQL Server `rowversion` + HTTP ETags
- File handling with secure uploads to **Azure Blob Storage** via SAS
- Deployment to **Azure App Service** with Azure SQL & Key Vault

---

## 📚 Learning Outcomes

By working through this project, students learn:

- How to implement **authentication** (JWT) and **authorization** (roles, claims).
- How to design entities with **many-to-many** relationships.
- How to enforce **optimistic concurrency** with ETags and `If-Match`.
- How to build flexible APIs with **query specs, filters, pagination**.
- How to handle **file uploads/downloads** securely in the cloud.
- How to configure and deploy an API to **Azure**.
- (Stretch) How to enable real-time updates with **SignalR**.

---

## 🏗 Entities

- **User** – registered account
- **Team** – grouping of users
- **TeamMembership** – user role per team (Owner, Member, Viewer)
- **Board** – belongs to a team
- **Column** – ordered columns inside a board
- **Card** – task item with assignees, labels, due date, comments
- **Label** – reusable tags
- **Comment** – discussion on cards
- **Attachment** – uploaded files (via Azure Blob)

---

## 🔑 Authentication & RBAC

- **JWT** access & refresh tokens
- Claims contain team memberships:
  ```json
  {
    "sub": "user-id",
    "email": "danny.lopez@devstack.com",
    "teams": [
      {"id": "T1", "role": "Owner"},
      {"id": "T2", "role": "Viewer"}
    ]
  }

---
### Migrations

### Add new migration
Replace `{{MigrationName}}` with the new migration name.
```bash
dotnet ef --startup-project .\BoardForgeAPI\ migrations add {{MigrationName}} --project .\Infrastructure.BoardForge -o Data/Migrations
```

### Update database
```bash
dotnet ef --startup-project .\BoardForgeAPI\ database update --project .\Infrastructure.BoardForge
```