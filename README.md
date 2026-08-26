# Movie Rental Management System

## Setup

### Backend
1. Clone the repository
2. Copy `Backend/MovieRental.Apis/appsettings.example.json` to `Backend/MovieRental.Apis/appsettings.json`
3. Fill in your database credentials and JWT secret key
4. Run `dotnet restore`
5. Run `dotnet run --project MovieRental.Apis`

### Frontend
1. Navigate to `Frontend` folder
2. Copy `.env.example` to `.env`
3. Fill in your API base URL
4. Run `npm install`
5. Run `npm run dev`

## Tech Stack
- **Backend**: ASP.NET Core, Entity Framework Core, PostgreSQL
- **Frontend**: React, PrimeReact, Vite
