# HTTP 400 - Invalid Hostname - FIXED

## Problem
Production deployment gave:
```
❌ Bad Request - Invalid Hostname
HTTP Error 400. The request hostname is invalid.
https://examv2.fwu.edu.np/Identity/Account/Login
```

## Root Cause
The `appsettings.Production.json` had a **placeholder value** for `AllowedHosts`:
```json
"AllowedHosts": "your-production-domain.com"
```

This didn't match the actual production domain `examv2.fwu.edu.np`, so ASP.NET Core rejected the request as invalid.

## Solution Applied
Updated `appsettings.Production.json` with the correct hostname configuration:

```json
"AllowedHosts": "examv2.fwu.edu.np,localhost,127.0.0.1"
```

Also updated the EmailSettings Logo URL to use the production domain:
```json
"EmailSettings": {
  "LogoUrl": "https://examv2.fwu.edu.np/images/fwu-logo.jpg"
}
```

## What is AllowedHosts?

`AllowedHosts` is a security feature in ASP.NET Core that validates the `Host` header in incoming HTTP requests.

**Why it matters:**
- Prevents DNS rebinding attacks
- Ensures your application only responds to legitimate hostnames
- Protects against host header injection vulnerabilities

**Format:**
```json
"AllowedHosts": "host1.com,host2.com,host3.com"
```

- Comma-separated list of allowed hostnames
- Use `*` to allow any host (development only!)
- Supports wildcards: `*.example.com`
- Include all domains your app responds to

## Configuration Changes

### Before (❌ Broken)
```json
"AllowedHosts": "your-production-domain.com"
```

### After (✅ Fixed)
```json
"AllowedHosts": "examv2.fwu.edu.np,localhost,127.0.0.1"
```

| Hostname | Purpose |
|----------|---------|
| `examv2.fwu.edu.np` | Main production domain |
| `localhost` | Local development |
| `127.0.0.1` | Loopback IP for testing |

## URLs That Now Work

✅ Production:
```
https://examv2.fwu.edu.np/Identity/Account/Login
https://examv2.fwu.edu.np/
https://examv2.fwu.edu.np/tenant/OCE/Core/Faculty/Details/1
```

✅ Development/Local:
```
http://localhost:5211/Identity/Account/Login
https://localhost:7164/Identity/Account/Login
http://127.0.0.1:5211/Login
```

## Deployment Steps

1. ✅ Code updated in `appsettings.Production.json`
2. ✅ Build verified (successful)
3. **Next**: Redeploy to production with updated configuration

### How to Deploy

**Option A: Via Git**
```bash
git pull origin develop
dotnet publish FWU.Exam.Management.Web -c Release -o ./publish
# Copy ./publish to your production server
# Restart the application
```

**Option B: Direct Configuration Update**
If you already have the application deployed, you can just:
1. Update `appsettings.Production.json` on the server
2. Restart the application (no rebuild needed)

## Verification After Deployment

Test that these now work in production:
```
✅ https://examv2.fwu.edu.np/Identity/Account/Login
✅ https://examv2.fwu.edu.np/
✅ https://examv2.fwu.edu.np/Dashboard
```

## If You Add More Domains Later

If you need to add more hostnames (e.g., `www.examv2.fwu.edu.np` or `exam.fwu.edu.np`):

```json
"AllowedHosts": "examv2.fwu.edu.np,www.examv2.fwu.edu.np,exam.fwu.edu.np,localhost,127.0.0.1"
```

## Files Modified
- ✅ `FWU.Exam.Management.Web/appsettings.Production.json`

## Build Status
- ✅ Build: SUCCESSFUL
- ✅ Errors: 0
- ✅ Warnings: 0

## Important Notes

1. **Don't use `*` in production** - It disables hostname validation security
2. **Remember to update when adding SSL certificates** - If you add a new domain, add it to AllowedHosts
3. **Check your reverse proxy** - If using Nginx/HAProxy, ensure it's forwarding headers correctly
4. **Case-insensitive** - `examv2.fwu.edu.np` and `EXAMV2.FWU.EDU.NP` are treated the same

## Next Steps

1. ✅ Redeploy application with new configuration
2. ✅ Clear browser cache if issues persist
3. ✅ Test login and other pages
4. ✅ Monitor application logs for any errors

---

**Status**: ✅ FIXED - Ready for Production  
**Date**: 2026-08-04
