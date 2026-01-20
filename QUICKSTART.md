# Quick Start Guide

Get your Sunderland RFC Accident Book up and running in minutes!

## Prerequisites Check

Before starting, ensure you have:
- ✅ Node.js (v18+) installed - Check with: `node --version`
- ✅ .NET 8 SDK installed - Check with: `dotnet --version`
- ✅ Angular CLI installed - Install with: `npm install -g @angular/cli`

## Step-by-Step Setup

### 1. Start the Backend API

Open a terminal and run:

```bash
cd backend/AccidentBook.API
dotnet restore
dotnet run
```

You should see:
```
Now listening on: http://localhost:5000
Now listening on: https://localhost:5001
```

✅ **Backend is running!** Keep this terminal open.

### 2. Start the Frontend

Open a **new** terminal and run:

```bash
cd frontend
npm install
ng serve
```

You should see:
```
✔ Browser application bundle generation complete.
** Angular Live Development Server is listening on localhost:4200 **
```

✅ **Frontend is running!**

### 3. Open the Application

Open your browser and navigate to:
**http://localhost:4200**

You should see the Accident Recording Book interface!

## First Steps

1. Click **"New Accident Record"** button
2. Fill in the required fields (marked with *)
3. Submit the form
4. View your accident records in the list

## Testing the API

You can also test the API directly using Swagger:
- Navigate to: **http://localhost:5000/swagger**
- Try the endpoints interactively

## Troubleshooting

### Backend won't start
- Make sure port 5000/5001 is not in use
- Check that .NET 8 SDK is installed correctly
- Run `dotnet --version` to verify

### Frontend won't start
- Make sure port 4200 is not in use
- Check that Node.js is installed: `node --version`
- Try deleting `node_modules` and running `npm install` again

### Can't connect to API
- Make sure the backend is running first
- Check the browser console for CORS errors
- Verify the API URL in `frontend/src/app/services/accident.service.ts`

### Database issues
- The SQLite database (`accidents.db`) is created automatically
- If you need to reset, delete `accidents.db` and restart the backend

## Next Steps

- Review the main [README.md](README.md) for detailed documentation
- Check deployment options for free hosting
- Consider adding authentication for production use

Happy coding! 🏉📋

