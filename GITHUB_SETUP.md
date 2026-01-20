# Getting Your Project on GitHub

This guide will walk you through getting your Sunderland RFC Accident Book project onto GitHub, which is required for free hosting deployment.

## Prerequisites

- A GitHub account (create one at [github.com](https://github.com) if you don't have one)
- Git installed on your computer (download from [git-scm.com](https://git-scm.com))

## Step 1: Check if Git is Installed

Open a terminal (PowerShell on Windows) and run:

```bash
git --version
```

If you see a version number (e.g., `git version 2.42.0`), you're good to go!

If not, download and install Git from [git-scm.com/downloads](https://git-scm.com/downloads)

## Step 2: Create a GitHub Account (If Needed)

1. Go to [github.com](https://github.com)
2. Click "Sign up"
3. Enter your email, password, and username
4. Verify your email address

## Step 3: Create a New Repository on GitHub

1. Log in to GitHub
2. Click the **"+"** icon in the top right → **"New repository"**
3. Fill in the details:
   - **Repository name:** `sunderland-rfc-accident-book` (or any name you prefer)
   - **Description:** "Accident recording system for Sunderland RFC"
   - **Visibility:** Choose **Private** (recommended) or **Public**
   - **DO NOT** check "Add a README file" (we already have one)
   - **DO NOT** add .gitignore or license (we already have these)
4. Click **"Create repository"**

## Step 4: Initialize Git in Your Project

Open a terminal in your project root directory (`D:\Development\AccidentBook`).

### Option A: Using Command Line (Recommended)

1. **Navigate to your project:**
   ```bash
   cd D:\Development\AccidentBook
   ```

2. **Initialize Git (if not already done):**
   ```bash
   git init
   ```

3. **Check if you have a .gitignore file:**
   ```bash
   dir .gitignore
   ```
   (We already created one, so this should exist)

4. **Add all your files:**
   ```bash
   git add .
   ```

5. **Make your first commit:**
   ```bash
   git commit -m "Initial commit - Sunderland RFC Accident Book"
   ```

6. **Set your name and email (if not already set):**
   ```bash
   git config --global user.name "Your Name"
   git config --global user.email "your.email@example.com"
   ```

7. **Connect to GitHub:**
   ```bash
   git remote add origin https://github.com/YOUR-USERNAME/sunderland-rfc-accident-book.git
   ```
   Replace `YOUR-USERNAME` with your actual GitHub username!

8. **Push to GitHub:**
   ```bash
   git branch -M main
   git push -u origin main
   ```

   You'll be prompted for your GitHub username and password. 
   **Note:** For password, you'll need to use a **Personal Access Token** (see Step 5 below).

### Option B: Using GitHub Desktop (Easier for Beginners)

1. **Download GitHub Desktop:**
   - Go to [desktop.github.com](https://desktop.github.com)
   - Download and install

2. **Sign in to GitHub Desktop:**
   - Open GitHub Desktop
   - Sign in with your GitHub account

3. **Add your local repository:**
   - Click **File** → **Add Local Repository**
   - Browse to `D:\Development\AccidentBook`
   - Click **Add repository**

4. **Commit your files:**
   - You'll see all your files listed
   - Enter a commit message: "Initial commit - Sunderland RFC Accident Book"
   - Click **"Commit to main"**

5. **Publish to GitHub:**
   - Click **"Publish repository"** button
   - Choose name and visibility
   - Click **"Publish repository"**

## Step 5: Create a Personal Access Token (For Command Line)

If using command line, GitHub requires a Personal Access Token instead of password:

1. **Go to GitHub Settings:**
   - Click your profile picture (top right)
   - Click **Settings**
   - Scroll down to **Developer settings** (left sidebar)
   - Click **Personal access tokens** → **Tokens (classic)**

2. **Generate New Token:**
   - Click **"Generate new token"** → **"Generate new token (classic)"**
   - Give it a name: "Accident Book Project"
   - Select expiration (90 days or no expiration)
   - Check **"repo"** scope (gives full repository access)
   - Click **"Generate token"**

3. **Copy the Token:**
   - **IMPORTANT:** Copy the token immediately (you won't see it again!)
   - Save it somewhere safe

4. **Use Token When Pushing:**
   - When prompted for password, paste the token instead

## Step 6: Verify Your Code is on GitHub

1. Go to your GitHub repository page
2. You should see all your files:
   - `README.md`
   - `backend/` folder
   - `frontend/` folder
   - All other project files

## Step 7: Keep Your Code Updated

Whenever you make changes:

### Using Command Line:
```bash
git add .
git commit -m "Description of your changes"
git push
```

### Using GitHub Desktop:
1. Make your changes
2. Open GitHub Desktop
3. See your changes in the left panel
4. Enter commit message
5. Click **"Commit to main"**
6. Click **"Push origin"** button

## Troubleshooting

### "fatal: not a git repository"
- Make sure you're in the project root directory
- Run `git init` first

### "remote origin already exists"
- You already connected to GitHub
- To change the URL: `git remote set-url origin https://github.com/YOUR-USERNAME/REPO-NAME.git`

### "Permission denied" or "Authentication failed"
- Make sure you're using a Personal Access Token, not your password
- Check that the token has "repo" scope enabled

### "Failed to push some refs"
- Someone else pushed changes (unlikely for new repo)
- Try: `git pull origin main` first, then `git push`

### Can't see my files on GitHub
- Make sure you ran `git add .` before committing
- Check that files aren't in `.gitignore`

## What Files Should Be on GitHub?

✅ **Include:**
- All source code files
- `README.md`
- `DEPLOYMENT.md`
- Configuration files (`.csproj`, `package.json`, etc.)
- `.gitignore`

❌ **Exclude (handled by .gitignore):**
- `node_modules/` folder
- `bin/` and `obj/` folders
- `*.db` database files
- `.vs/` Visual Studio files
- `dist/` build output

## Next Steps

Once your code is on GitHub:

1. ✅ Your code is backed up
2. ✅ You can deploy to Render/Netlify (see DEPLOYMENT.md)
3. ✅ You can collaborate with others
4. ✅ You can track changes over time

## Quick Reference Commands

```bash
# Check status
git status

# Add all changes
git add .

# Commit changes
git commit -m "Your message here"

# Push to GitHub
git push

# Pull latest changes
git pull

# See commit history
git log

# Check remote connection
git remote -v
```

## Need Help?

- **Git Documentation:** [git-scm.com/doc](https://git-scm.com/doc)
- **GitHub Guides:** [guides.github.com](https://guides.github.com)
- **GitHub Desktop Help:** [help.github.com/desktop](https://help.github.com/desktop)

---

**You're all set!** Once your code is on GitHub, you can proceed with deployment using the instructions in `DEPLOYMENT.md`.

