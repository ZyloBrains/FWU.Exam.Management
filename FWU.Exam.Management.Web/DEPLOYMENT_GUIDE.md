# FWU Exam Management - Production Deployment Guide

## Pre-Deployment Checklist

### 1. Database Setup
- [ ] Ensure SQL Server is accessible from the deployment environment
- [ ] Create the database: `FWU.Exams`
- [ ] Apply migrations before deploying the application

**Applying Migrations:**
```bash
# Using Entity Framework CLI
dotnet ef database update --project FWU.Exam.Management.Infrastructure --startup-project FWU.Exam.Management.Web

# Or from Package Manager Console
Update-Database -Project FWU.Exam.Management.Infrastructure -StartupProject FWU.Exam.Management.Web
```

### 2. Environment Configuration
- [ ] Set `ASPNETCORE_ENVIRONMENT` to `Production`
- [ ] Update `appsettings.Production.json` with:
  - Production SQL Server connection string
  - Production domain for `AllowedHosts`
  - Email settings (LogoUrl, SMTP configuration)
  - Payment gateway credentials (Khalti, eSewa, etc.)

### 3. Security Requirements
- [ ] Enable HTTPS/SSL certificates
- [ ] Configure TLS 1.2 minimum
- [ ] Update `AllowedHosts` in appsettings.Production.json
- [ ] Configure firewall rules
- [ ] Disable debugging endpoints in production

### 4. Application Settings
- [ ] Review and update all configuration values in Core configurations:
  - SMTP Configuration (Email)
  - SMS Configuration (GumpNow or SMS provider)
  - Payment Configurations (Khalti, eSewa, ConnectIPS)
  - Academic Years, Faculties, Programs, Colleges

### 5. Logging and Monitoring
- [ ] Configure log file location (ensure write permissions)
- [ ] Set up monitoring and alerting
- [ ] Configure centralized logging if needed
- [ ] Review log retention policies

### 6. File Storage
- [ ] Ensure write permissions for:
  - `/wwwroot/images/` - Faculty logos
  - `/wwwroot/organization/` - Organization logos
  - `/wwwroot/uploads/photos/` - Student photos
  - `/wwwroot/uploads/signatures/` - Digital signatures
- [ ] Configure backup strategy for uploaded files

### 7. Identity and Authentication
- [ ] Configure email sender credentials
- [ ] Ensure email confirmation is properly configured
- [ ] Set up password reset functionality
- [ ] Review password policies
- [ ] Enable two-factor authentication if needed

### 8. Payment Gateway Integration
- [ ] Configure Khalti credentials
- [ ] Configure eSewa credentials
- [ ] Configure ConnectIPS credentials
- [ ] Test payment flows in staging environment

## Deployment Steps

### 1. Build the Application
```bash
dotnet build FWU.Exam.Management.sln -c Release
```

### 2. Publish the Application
```bash
dotnet publish FWU.Exam.Management.Web -c Release -o ./publish
```

### 3. Apply Database Migrations (if not already done)
```bash
dotnet ef database update --project FWU.Exam.Management.Infrastructure
```

### 4. Configure Application Server
- [ ] Copy published files to production server
- [ ] Set environment variables:
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `ASPNETCORE_URLS=https://+:443;http://+:80`
- [ ] Configure automatic HTTPS redirection
- [ ] Set up application pool/service with proper identity

### 5. Verify Deployment
- [ ] Health check endpoint: `/` should load
- [ ] Identity pages accessible: `/Identity/Account/Login`
- [ ] Area routes working: `/Students/StudentDashboard`
- [ ] API Swagger disabled (check by trying `/swagger`)
- [ ] HTTPS redirection working
- [ ] Security headers present (check browser dev tools)

## Post-Deployment Verification

### Critical Routes to Test
- [ ] Home Page: `https://yourdomain.com/`
- [ ] Login: `https://yourdomain.com/Identity/Account/Login`
- [ ] Student Dashboard: `https://yourdomain.com/Students/StudentDashboard/Profile`
- [ ] Admin Area: `https://yourdomain.com/Admin/Users`
- [ ] Core Settings: `https://yourdomain.com/Core/AcademicYears`

### Security Checks
- [ ] HTTPS is enforced (HTTP redirects to HTTPS)
- [ ] HSTS header is present
- [ ] Security headers are present:
  - X-Content-Type-Options: nosniff
  - X-Frame-Options: DENY
  - Content-Security-Policy
  - Strict-Transport-Security
- [ ] Swagger UI is not accessible
- [ ] Developer exception page is not shown

### Performance Checks
- [ ] Application starts within acceptable time
- [ ] Database queries are performing well
- [ ] File uploads work correctly
- [ ] Email sending works
- [ ] Payment gateway integration works

## Troubleshooting

### Connection String Issues
- Verify SQL Server is accessible from application server
- Check authentication (AAD Managed Identity vs SQL Auth)
- Ensure database exists and migrations are applied

### HTTPS/Certificate Issues
- Verify SSL certificate is installed and valid
- Check certificate binding in IIS or reverse proxy
- Ensure proper certificate chain

### Performance Issues
- Check database index usage
- Review Entity Framework query patterns
- Monitor application logs for errors
- Check CPU and memory usage

### Email Configuration Issues
- Verify SMTP credentials in appsettings
- Check firewall rules for SMTP port (25, 587, 465)
- Review email templates
- Check authenticated sender address

## Rollback Procedure

If issues occur post-deployment:

1. Revert to previous application version
2. Keep database unchanged (avoid downtime)
3. Investigate issues in staging environment
4. Fix and re-test before re-deployment

## Maintenance

### Regular Tasks
- [ ] Monitor application logs
- [ ] Monitor database growth and optimize indexes
- [ ] Review and archive old audit logs
- [ ] Apply security patches to dependencies
- [ ] Backup database regularly
- [ ] Backup file uploads regularly

### Database Maintenance
```sql
-- Check database size
EXEC sp_spaceused;

-- Check index fragmentation
SELECT * FROM sys.dm_db_index_physical_stats;

-- Rebuild fragmented indexes
ALTER INDEX index_name ON table_name REBUILD;
```

## Support Contacts

For issues or questions regarding deployment:
- Check application logs in `/logs/`
- Review Azure diagnostic logs
- Contact DevOps team for infrastructure issues
