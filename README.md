# Quest Bound

**Quest Bound** is a social app that assigns a daily “side quest” to one member of your friend group. The selected player has until midnight to complete the challenge, document it, and submit their result. The group then votes on whether the quest was successfully completed.

Try it out: [https://questbound.vercel.app](https://questbound.vercel.app)

## Features

* Create and join friend groups
* Receive personalized, AI-generated daily quests
* Vote on quest completion as a group

## Tech Stack

### Backend

* ASP.NET Core Web API
* Entity Framework Core
* AutoMapper
* Swagger
* xUnit

### Database & Storage

* PostgreSQL
* Supabase

### Frontend

* React (TypeScript)
* Vite
* Customized shadcn/ui

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/gabigoranov/QuestGiver.git
cd QuestGiver
```

### 2. Install Dependencies

#### Backend

* Install .NET 8 SDK

```bash
cd API
dotnet restore
```

#### Frontend

```bash
cd Frontend
npm install
```

### 3. Configure Environment

Create an `appsettings.json` file in the backend project and provide:

* API keys
* Database connection strings
* Supabase configuration
* Any other required secrets

### 4. Run the Application

#### Backend

```bash
dotnet run
```

#### Frontend

```bash
npm run dev
```

## Notes

* Ensure PostgreSQL and Supabase are properly configured before running the app
* You may need to update environment variables depending on your setup

