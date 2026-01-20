# Free Hosting Deployment Guide

This guide provides step-by-step instructions for hosting your Sunderland RFC Accident Book application for free.

## Recommended Setup

**Best Free Option:**
- **Frontend:** Netlify (easiest, best free tier)
- **Backend:** Render (reliable, good free tier)
- **Database:** SQLite (included with backend)

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

## Option 2: Vercel (Frontend) + Railway (Backend)

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
| **Azure** | ⚠️ Limited | Backend | Complex, limited free tier |

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

**Always-On Option:**
- Netlify Frontend: ✅ Free
- Render Backend: $7/month (always-on)
- Total: **$7/month**

---

## Next Steps After Deployment

1. ✅ Test all functionality
2. ✅ Set up custom domain
3. ✅ Configure backups (export data regularly)
4. ✅ Add authentication (recommended for production)
5. ✅ Monitor usage and performance

---

## Support Resources

- **Netlify Docs:** https://docs.netlify.com
- **Render Docs:** https://render.com/docs
- **Railway Docs:** https://docs.railway.app
- **Vercel Docs:** https://vercel.com/docs

Need help? Check the service-specific documentation or community forums!

