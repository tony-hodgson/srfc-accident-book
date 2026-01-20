# Sunderland RFC Accident Book

An online accident recording system for Sunderland RFC, built with Angular and ASP.NET Core.

## Features

- Simple, user-friendly accident reporting form
- Secure data storage
- Free hosting compatible (SQLite database)
- Modern UI with Bootstrap styled in Sunderland RFC colors

## Project Structure

```
AccidentBook/
├── frontend/          # Angular application
├── backend/           # ASP.NET Core Web API
└── README.md
```

## Prerequisites

### Frontend
- Node.js (v18 or higher)
- Angular CLI: `npm install -g @angular/cli`

### Backend
- .NET 8 SDK or higher
- Visual Studio 2022 or VS Code

## Setup Instructions

### Backend Setup

1. Navigate to the backend API directory:
   ```bash
   cd backend/AccidentBook.API
   ```

2. Restore packages:
   ```bash
   dotnet restore
   ```

3. Run the API:
   ```bash
   dotnet run
   ```

   The API will be available at:
   - `https://localhost:5001` (HTTPS)
   - `http://localhost:5000` (HTTP)
   - Swagger UI: `http://localhost:5000/swagger` (in development)

   **Note:** The SQLite database (`accidents.db`) will be created automatically in the API directory on first run.
   
   **Important:** If you have an existing database and are adding the Age field, you may need to delete the `accidents.db` file to recreate it with the new schema, or manually add the Age column using a SQLite tool.

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   ng serve
   ```

   The application will be available at `http://localhost:4200`

   **Note:** Make sure the backend API is running before using the frontend, or you'll see connection errors.

### Running Both Together

1. Open two terminal windows
2. In the first terminal, start the backend:
   ```bash
   cd backend/AccidentBook.API
   dotnet run
   ```
3. In the second terminal, start the frontend:
   ```bash
   cd frontend
   ng serve
   ```
4. Open your browser to `http://localhost:4200`

## Free Hosting Options

### Backend
- **Azure App Service** (Free tier available)
- **Railway** (Free tier with credit card)
- **Render** (Free tier available)
- **Fly.io** (Free tier available)

### Frontend
- **Netlify** (Free tier)
- **Vercel** (Free tier)
- **GitHub Pages** (Free)

### Database
- SQLite (included, works with all hosting options)
- **Supabase** (Free PostgreSQL tier)
- **Railway** (Free PostgreSQL tier)

## API Endpoints

- `GET /api/accidents` - Get all accident records
- `GET /api/accidents/{id}` - Get a specific accident record
- `POST /api/accidents` - Create a new accident record
- `PUT /api/accidents/{id}` - Update an accident record
- `DELETE /api/accidents/{id}` - Delete an accident record

## Getting Started

### First Time Setup

1. **Get Your Code on GitHub** (Required for deployment)
   - See [GITHUB_SETUP.md](GITHUB_SETUP.md) for step-by-step instructions
   - This is required before deploying to free hosting services

2. **Deploy Your Application**
   - See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed deployment instructions

## Deployment Instructions

📖 **For detailed step-by-step deployment instructions, see [DEPLOYMENT.md](DEPLOYMENT.md)**

### Quick Summary

**Recommended Free Setup:**
- **Frontend:** Deploy to [Netlify](https://netlify.com) (free, easy)
- **Backend:** Deploy to [Render](https://render.com) (free tier available)
- **Database:** SQLite (included with backend)

### Quick Deploy Steps

1. **Backend (Render):**
   - Push code to GitHub
   - Create new Web Service on Render
   - Point to `backend/AccidentBook.API`
   - Deploy (takes ~5-10 minutes)

2. **Frontend (Netlify):**
   - Update `frontend/src/environments/environment.prod.ts` with your API URL
   - Build: `ng build --configuration production`
   - Drag `dist/accident-book` folder to Netlify
   - Done!

See [DEPLOYMENT.md](DEPLOYMENT.md) for complete instructions with screenshots and troubleshooting.

**Important:** For production, consider:
- Adding authentication/authorization
- Using environment variables for sensitive configuration
- Setting up proper database backups
- Enabling HTTPS only

## Security Notes

- All data is stored securely in the database
- CORS is configured for the frontend domain
- Input validation on both frontend and backend
- Consider adding authentication for production use
- Update CORS origins in `appsettings.json` for production
- Update API URL in `frontend/src/environments/environment.prod.ts` for production

## License

This project is for internal use by the rugby club.

