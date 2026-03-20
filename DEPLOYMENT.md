# Free Hosting Deployment Guide

This guide provides step-by-step instructions for hosting your Sunderland RFC Accident Book application for free.

## Recommended Setup

**Best Free Option:**
- **Frontend:** Netlify (easiest, best free tier) or Azure Static Web Apps (all Azure)
- **Backend:** Azure App Service (free tier, always-on) or Render (reliable, good free tier)
- **Database:** SQLite (included with backend)

**Quick Start - Azure:**
- See [Option 2: Azure App Service](#option-2-netlify-frontend--azure-app-service-backend---recommended-for-azure) below
- Free tier (F1) includes: Always available, 1 GB storage, 60 minutes compute/day
- No credit card charges if you stay on free tier

## Option 1: Netlify (Frontend) + Render (Backend) - RECOMMENDED

### Part 1: Deploy Backend to Render

1. **Create a Render Account**
   - Go to [render.com](https://render.com)
   - Sign up with GitHub (recommended) or email
   - Free tier includes 750 hours/month

2. **Prepare Your Backend**
   - Push your code to GitHub (if not already)
   - Make sure `backend/AccidentBook.API` is in your repository

3. **Create a New Web Service on Render**
   - Click "New +" → "Web Service"
   - Connect your GitHub repository
   - Select your repository
   - Configure:
     - **Name:** `sunderland-rfc-accident-api`
     - **Environment:** `Docker` or `.NET`
     - **Build Command:** `cd backend/AccidentBook.API && dotnet restore && dotnet publish -c Release -o ./publish`
     - **Start Command:** `cd backend/AccidentBook.API/publish && dotnet AccidentBook.API.dll`
     - **Plan:** Free
   
   **OR use Render's .NET detection:**
   - Render will auto-detect .NET if you point it to `backend/AccidentBook.API`
   - Build Command: `dotnet restore && dotnet publish -c Release -o ./publish`
   - Start Command: `./publish/AccidentBook.API`

4. **Configure Environment Variables**
   - In Render dashboard, go to "Environment"
   - Add if needed:
     - `ASPNETCORE_ENVIRONMENT=Production`
     - `ASPNETCORE_URLS=http://0.0.0.0:10000` (Render's default port)

5. **Update CORS Settings**
   - In `backend/AccidentBook.API/appsettings.json`, update:
   ```json
   {
     "AllowedOrigins": [
       "https://your-netlify-app.netlify.app",
       "http://localhost:4200"
     ]
   }
   ```
   - You'll update this with your actual Netlify URL after deployment

6. **Deploy**
   - Click "Create Web Service"
   - Wait for deployment (5-10 minutes)
   - Copy your service URL (e.g., `https://sunderland-rfc-accident-api.onrender.com`)

**Important Notes for Render:**
- Free tier services spin down after 15 minutes of inactivity
- First request after spin-down may take 30-60 seconds
- Consider upgrading to keep service always on (optional)

### Part 2: Deploy Frontend to Netlify

1. **Create a Netlify Account**
   - Go to [netlify.com](https://netlify.com)
   - Sign up with GitHub (recommended)

2. **Update Production Environment**
   - Edit `frontend/src/environments/environment.prod.ts`:
   ```typescript
   export const environment = {
     production: true,
     apiUrl: 'https://your-render-api-url.onrender.com/api'
   };
   ```
   - Replace with your actual Render API URL

3. **Build Your Frontend**
   ```bash
   cd frontend
   npm install
   ng build --configuration production
   ```
   - This creates `frontend/dist/accident-book` folder

4. **Deploy to Netlify**
   
   **Option A: Drag & Drop (Easiest)**
   - Go to Netlify dashboard
   - Drag the `frontend/dist/accident-book` folder to Netlify
   - Your site will be live immediately!
   - Copy the URL (e.g., `https://random-name-123.netlify.app`)

   **Option B: GitHub Integration (Recommended)**
   - Push your code to GitHub
   - In Netlify: "Add new site" → "Import an existing project"
   - Connect GitHub and select your repository
   - Configure:
     - **Base directory:** `frontend`
     - **Build command:** `npm install && ng build --configuration production`
     - **Publish directory:** `frontend/dist/accident-book`
   - Click "Deploy site"

5. **Update CORS in Backend**
   - Go back to Render dashboard
   - Update `appsettings.json` with your Netlify URL
   - Redeploy the backend

6. **Set Custom Domain (Optional)**
   - In Netlify: Site settings → Domain management
   - Add your custom domain (e.g., `accidents.sunderlandrugby.com`)

---

## Option 2: Netlify (Frontend) + Azure App Service (Backend) - RECOMMENDED FOR AZURE

Azure App Service offers a free tier (F1) that is always-on (unlike Render which spins down). This is great for production use.

**Quick Summary:**
- ✅ Free tier (F1): Always available, 1 GB storage, 60 minutes compute/day
- ✅ No credit card charges if you stay on free tier
- ✅ Free SSL certificate included
- ✅ Custom domain support
- ⚠️ May sleep after 20 minutes inactivity (first request: 10-30 seconds)
- 📖 **Quick Reference:** See [AZURE_DEPLOYMENT.md](AZURE_DEPLOYMENT.md) for condensed guide

### Part 1: Deploy Backend to Azure App Service

#### Prerequisites

1. **Azure Account**
   - Go to [azure.microsoft.com](https://azure.microsoft.com)
   - Sign up for free (requires credit card but won't be charged for free tier)
   - Get $200 free credit for 30 days (optional, not needed for free tier)

2. **Azure CLI** (Optional but recommended)
   - Download from [aka.ms/installazurecliwindows](https://aka.ms/installazurecliwindows)
   - Or use Azure Portal (web interface)

#### Method A: Deploy via Azure Portal (Easiest)

1. **Create App Service**
   - Go to [portal.azure.com](https://portal.azure.com)
   - Click "Create a resource"
   - Search for "Web App" or "App Service"
   - Click "Create"

2. **Configure Basic Settings**
   - **Subscription:** Choose your subscription (Free tier available)
   - **Resource Group:** Create new (e.g., `sunderland-rfc-accident-book`)
   - **Name:** `sunderland-rfc-accident-api` (must be globally unique)
   - **Publish:** Code
   - **Runtime stack:** `.NET 8 (LTS)`
   - **Operating System:** Linux (recommended) or Windows
   - **Region:** Choose closest to you (e.g., `UK South`)
   - **App Service Plan:** 
     - Click "Create new"
     - Name: `sunderland-rfc-plan`
     - **SKU and size:** `Free F1` (1 GB RAM, 1 GB storage)
     - Click "OK"

3. **Review and Create**
   - Review settings
   - Click "Create"
   - Wait for deployment (2-5 minutes)

4. **Configure Deployment**
   - Once created, go to your App Service
   - In left menu: **Deployment Center**
   - **Source:** GitHub
   - Authorize GitHub if needed
   - Select your repository and branch
   - **Build provider:** GitHub Actions (recommended) or App Service build service
   - **Workflow file path:** `.github/workflows/azure-deploy.yml` (will be created)
   - Click "Save"

5. **Configure Application Settings**
   - Go to **Configuration** → **Application settings**
   - Add these settings:
     ```
     ASPNETCORE_ENVIRONMENT = Production
     ASPNETCORE_URLS = http://+:8080
     ```
   - For CORS, add:
     ```
     AllowedOrigins__0 = https://srfcaccidentbook.netlify.app
     AllowedOrigins__1 = http://localhost:4200
     ```
     (Note: Use double underscore `__` for array indices in Azure)
   - Click "Save"

6. **Configure Authentication (JWT)**
   - Add to Application settings:
     ```
     Jwt__Key = YourSecureJWTKeyHereMinimum32Characters
     Jwt__Issuer = SunderlandRFCAccidentBook
     Jwt__Audience = SunderlandRFCAccidentBook
     ```
   - **Important:** Generate a secure key (see Security section below)

7. **Configure Google OAuth (Optional)**
   - Add to Application settings:
     ```
     Google__ClientId = your-google-client-id
     Google__ClientSecret = your-google-client-secret
     ```

8. **Get Your URL**
   - Go to **Overview** in your App Service
   - Copy the URL (e.g., `https://sunderland-rfc-accident-api.azurewebsites.net`)

#### Method B: Deploy via Azure CLI

1. **Login to Azure**
   ```bash
   az login
   ```

2. **Create Resource Group**
   ```bash
   az group create --name sunderland-rfc-rg --location uksouth
   ```

3. **Create App Service Plan (Free Tier)**
   ```bash
   az appservice plan create --name sunderland-rfc-plan --resource-group sunderland-rfc-rg --sku FREE --is-linux
   ```

4. **Create Web App**
   ```bash
   az webapp create --resource-group sunderland-rfc-rg --plan sunderland-rfc-plan --name sunderland-rfc-accident-api --runtime "DOTNET|8.0"
   ```

5. **Configure Deployment from GitHub**
   ```bash
   az webapp deployment source config --name sunderland-rfc-accident-api --resource-group sunderland-rfc-rg --repo-url https://github.com/YOUR-USERNAME/YOUR-REPO --branch main --manual-integration
   ```

6. **Set Application Settings**
   ```bash
   az webapp config appsettings set --resource-group sunderland-rfc-rg --name sunderland-rfc-accident-api --settings ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS="http://+:8080"
   ```

7. **Set CORS Origins**
   ```bash
   az webapp cors add --resource-group sunderland-rfc-rg --name sunderland-rfc-accident-api --allowed-origins "https://srfcaccidentbook.netlify.app" "http://localhost:4200"
   ```

#### Create GitHub Actions Workflow (Auto-Deploy)

Create `.github/workflows/azure-deploy.yml`:

```yaml
name: Deploy to Azure App Service

on:
  push:
    branches:
      - main
    paths:
      - 'backend/**'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Set up .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Build with dotnet
      run: dotnet build backend/AccidentBook.API/AccidentBook.API.csproj --configuration Release
    
    - name: Publish with dotnet
      run: dotnet publish backend/AccidentBook.API/AccidentBook.API.csproj --configuration Release --output ./publish
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'sunderland-rfc-accident-api'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

**To get the publish profile:**
1. Go to Azure Portal → Your App Service
2. **Get publish profile** (download button)
3. Copy contents
4. Go to GitHub → Your repo → Settings → Secrets → Actions
5. Add secret: `AZURE_WEBAPP_PUBLISH_PROFILE` (paste publish profile content)

### Part 2: Deploy Frontend to Netlify (or Azure Static Web Apps)

#### Option A: Netlify (Easiest)

Follow the Netlify instructions from Option 1, but use your Azure App Service URL in `environment.prod.ts`:
```typescript
apiUrl: 'https://sunderland-rfc-accident-api.azurewebsites.net/api'
```

#### Option B: Azure Static Web Apps (All Azure)

1. **Create Static Web App**
   - Go to Azure Portal
   - Create resource → "Static Web App"
   - Configure:
     - **Name:** `sunderland-rfc-accident-frontend`
     - **Resource Group:** Same as backend
     - **Plan:** Free
     - **Deployment details:** GitHub
     - **Organization/Repository:** Your GitHub repo
     - **Branch:** `main`
     - **Build presets:** Custom
     - **App location:** `frontend`
     - **Api location:** (leave empty)
     - **Output location:** `dist/accident-book`

2. **Configure Build**
   - Azure will create a GitHub Action workflow
   - Update it if needed:
     ```yaml
     app_location: "/frontend"
     api_location: ""
     output_location: "dist/accident-book"
     app_build_command: "npm install && ng build --configuration production"
     ```

3. **Get Your URL**
   - Azure provides: `https://sunderland-rfc-accident-frontend.azurestaticapps.net`

4. **Update CORS in Backend**
   - Add Static Web App URL to allowed origins in Azure App Service

### Azure App Service Configuration

#### Important Settings

1. **Always On** (Free tier doesn't support this, but service stays available)
   - Free tier: Service may sleep after 20 minutes of inactivity
   - First request after sleep: 10-30 seconds

2. **HTTPS Only**
   - Go to **TLS/SSL settings**
   - Enable "HTTPS Only"

3. **Application Insights** (Optional)
   - Free tier includes basic monitoring
   - Enable in **Application Insights** section

4. **Database Storage**
   - SQLite file stored in `/home` directory
   - Persists across deployments
   - **Backup:** Consider exporting data regularly

### Azure Free Tier Limitations

- **Compute:** 60 minutes/day (usually enough for small apps)
- **Storage:** 1 GB
- **Memory:** 1 GB RAM
- **No Always On:** Service may sleep after inactivity
- **Custom domains:** Supported with free SSL

### Cost

**Free Tier (F1):**
- ✅ Always free (no credit card charges if you stay on F1)
- ✅ 1 GB storage
- ✅ 60 minutes compute/day
- ✅ Free SSL certificate
- ✅ Custom domain support

**If you exceed free tier:**
- Automatically stops (won't charge you)
- Or upgrade to Basic tier ($13/month) for always-on

---

## Option 3: Vercel (Frontend) + Railway (Backend)

### Deploy Backend to Railway

1. **Create Railway Account**
   - Go to [railway.app](https://railway.app)
   - Sign up with GitHub
   - Free tier: $5 credit/month (usually enough for small apps)

2. **Create New Project**
   - Click "New Project"
   - Select "Deploy from GitHub repo"
   - Choose your repository

3. **Configure Service**
   - Railway auto-detects .NET
   - Set root directory: `backend/AccidentBook.API`
   - Railway will build and deploy automatically

4. **Get Your URL**
   - Railway provides a URL like `https://your-app.up.railway.app`
   - Copy this for your frontend configuration

### Deploy Frontend to Vercel

1. **Create Vercel Account**
   - Go to [vercel.com](https://vercel.com)
   - Sign up with GitHub

2. **Import Project**
   - Click "Add New" → "Project"
   - Import from GitHub
   - Configure:
     - **Root Directory:** `frontend`
     - **Build Command:** `npm install && ng build --configuration production`
     - **Output Directory:** `dist/accident-book`

3. **Environment Variables**
   - Add: `API_URL` = your Railway backend URL
   - Update `environment.prod.ts` to use this

4. **Deploy**
   - Click "Deploy"
   - Vercel provides a URL automatically

---

## Option 3: All-in-One with Fly.io

Fly.io can host both frontend and backend, but requires more setup.

1. **Install Fly CLI**
   ```bash
   # Windows (PowerShell)
   iwr https://fly.io/install.ps1 -useb | iex
   ```

2. **Create Fly.io Account**
   - Go to [fly.io](https://fly.io)
   - Sign up and install CLI

3. **Deploy Backend**
   ```bash
   cd backend/AccidentBook.API
   fly launch
   ```
   - Follow prompts
   - Free tier: 3 shared-cpu VMs

4. **Deploy Frontend**
   - Create separate Fly app for frontend
   - Or use Netlify/Vercel for frontend (easier)

---

## Quick Comparison

| Service | Free Tier | Best For | Limitations |
|---------|-----------|----------|-------------|
| **Netlify** | ✅ Generous | Frontend | 100GB bandwidth/month |
| **Vercel** | ✅ Generous | Frontend | Similar to Netlify |
| **Render** | ✅ Good | Backend | Spins down after 15min idle |
| **Railway** | ⚠️ $5 credit | Backend | Credit-based, may cost |
| **Fly.io** | ✅ 3 VMs | Both | More complex setup |
| **Azure App Service** | ✅ Free F1 | Backend | Always available, 60min/day compute |
| **Azure Static Web Apps** | ✅ Free | Frontend | Generous free tier |

---

## Step-by-Step: Recommended Setup (Netlify + Render)

### 1. Prepare Your Code

```bash
# Make sure everything is committed to Git
git add .
git commit -m "Ready for deployment"
git push origin main
```

### 2. Deploy Backend First

1. Go to render.com → Sign up
2. New → Web Service
3. Connect GitHub → Select repo
4. Settings:
   - Name: `sunderland-accident-api`
   - Root Directory: `backend/AccidentBook.API`
   - Build: `dotnet restore && dotnet publish -c Release -o ./publish`
   - Start: `./publish/AccidentBook.API`
   - Plan: Free
5. Deploy → Wait for URL (e.g., `https://sunderland-accident-api.onrender.com`)

### 3. Update Frontend Configuration

Edit `frontend/src/environments/environment.prod.ts`:
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://sunderland-accident-api.onrender.com/api'
};
```

### 4. Deploy Frontend

1. Go to netlify.com → Sign up
2. Sites → Add new site → Deploy manually
3. Build locally:
   ```bash
   cd frontend
   npm install
   ng build --configuration production
   ```
4. Drag `frontend/dist/accident-book` folder to Netlify
5. Get your URL (e.g., `https://sunderland-accident-book.netlify.app`)

### 5. Update CORS

1. Go back to Render dashboard
2. Environment → Add:
   ```
   AllowedOrigins=["https://sunderland-accident-book.netlify.app"]
   ```
3. Or update `appsettings.json` and redeploy

### 6. Test Your Deployment

- Visit your Netlify URL
- Try creating an accident record
- Check that data persists

---

## Important Notes

### Database Persistence

- **SQLite on Render/Railway:** Data persists in the service's filesystem
- **Free tier limitations:** Data may be lost if service is deleted
- **Backup recommendation:** Export data regularly or upgrade to paid tier

### CORS Configuration

Make sure your backend allows your frontend domain:
- Update `appsettings.json` in backend
- Or use environment variables in Render/Railway

### Environment Variables

For production, use environment variables instead of hardcoding:
- Render: Environment tab in dashboard
- Netlify: Site settings → Environment variables

### Custom Domains

Both Netlify and Render support custom domains:
- Netlify: Free SSL automatically
- Render: Free SSL on custom domains

---

## Troubleshooting

### Backend Not Responding
- Check Render/Railway logs
- Verify build completed successfully
- Check CORS settings

### Frontend Can't Connect to API
- Verify API URL in `environment.prod.ts`
- Check CORS configuration in backend
- Test API directly in browser (should see Swagger or error)

### Database Issues
- SQLite file is created automatically
- On Render free tier, data persists but service may spin down
- Consider using external database (Supabase free tier) for production

---

## Cost Summary

**Completely Free Option:**
- Netlify Frontend: ✅ Free
- Render Backend: ✅ Free (with spin-down)
- Total: **$0/month**

**Always-On Free Option (Azure):**
- Netlify Frontend: ✅ Free
- Azure App Service Backend: ✅ Free (F1 tier, always available)
- Total: **$0/month**
- ⚠️ Note: 60 minutes compute/day limit, may sleep after 20min inactivity

**Always-On Paid Option:**
- Netlify Frontend: ✅ Free
- Azure App Service Backend: $13/month (Basic B1, always-on)
- OR Render Backend: $7/month (always-on)
- Total: **$7-13/month**

---

## Next Steps After Deployment

1. ✅ Test all functionality
2. ✅ Set up custom domain
3. ✅ Configure backups (export data regularly)
4. ✅ Add authentication (recommended for production)
5. ✅ Monitor usage and performance

---

## Azure App Service Specific Notes

### Port Configuration

Azure App Service uses the `PORT` environment variable. Your app should listen on:
- `http://0.0.0.0:${PORT}` or `http://+:8080`
- The `Program.cs` is already configured to work with Azure

### Database Location

SQLite database is stored in:
- Linux: `/home` directory (persists across deployments)
- Windows: `D:\home` directory

**Important:** Data persists, but consider regular backups.

### Scaling

Free tier (F1):
- Single instance
- 1 GB RAM
- 1 GB storage
- 60 minutes compute/day

If you need more:
- Basic B1: $13/month (always-on, 1.75 GB RAM)
- Standard S1: $73/month (always-on, 1.75 GB RAM, auto-scale)

### Monitoring

- **Application Insights:** Free tier includes basic monitoring
- **Logs:** View in Azure Portal → App Service → Log stream
- **Metrics:** CPU, memory, requests in Overview

### Custom Domain

1. Go to **Custom domains** in App Service
2. Add your domain (e.g., `api.sunderlandrugby.com`)
3. Azure provides free SSL certificate
4. Update DNS records as instructed

### Troubleshooting Azure Deployment

**Build fails:**
- Check GitHub Actions logs (if using)
- Verify .NET 8 SDK is available
- Check build logs in Azure Portal → Deployment Center

**App not starting:**
- Check **Log stream** in Azure Portal
- Verify environment variables are set
- Check **Diagnose and solve problems** tool

**Slow first request:**
- Free tier may sleep after 20 minutes
- First request: 10-30 seconds
- Subsequent requests: Normal speed

**Database not persisting:**
- Verify database path uses `/home` (Linux) or `D:\home` (Windows)
- Check file permissions
- Consider using Azure SQL Database (paid) for production

---

## Support Resources

- **Netlify Docs:** https://docs.netlify.com
- **Render Docs:** https://render.com/docs
- **Railway Docs:** https://docs.railway.app
- **Vercel Docs:** https://vercel.com/docs
- **Azure App Service Docs:** https://docs.microsoft.com/azure/app-service
- **Azure Static Web Apps Docs:** https://docs.microsoft.com/azure/static-web-apps

Need help? Check the service-specific documentation or community forums!

