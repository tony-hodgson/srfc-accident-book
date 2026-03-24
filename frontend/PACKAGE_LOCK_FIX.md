# Fixing package-lock.json Sync Issues

## The Problem

Netlify was failing with:
```
npm error Missing: @popperjs/core@2.11.8 from lock file
```

This happens when `package-lock.json` is out of sync with `package.json`.

## The Solution

1. **Added @popperjs/core to package.json** - Bootstrap requires it as a peer dependency
2. **Changed build command** - Using `npm install` instead of `npm ci` for now

## To Properly Fix (Recommended)

Run these commands locally to regenerate the lock file:

```bash
cd frontend
rm package-lock.json
npm install
git add package-lock.json
git commit -m "Update package-lock.json with @popperjs/core"
git push
```

Then you can change back to `npm ci` in netlify.toml for faster, more reliable builds.

## Why This Happened

Bootstrap 5.3.2 has `@popperjs/core` as a peer dependency. When you install Bootstrap, npm automatically installs peer dependencies, but if they're not explicitly in `package.json`, the lock file can get out of sync.

By adding `@popperjs/core` explicitly to `package.json`, we ensure it's always tracked properly.

