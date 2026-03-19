# Deployment Checklist

Use this checklist when deploying your Stock Trading Platform to production.

## Pre-Deployment

### .NET API Backend

- [ ] Database is set up and migrations applied
- [ ] Connection strings configured for production
- [ ] Finnhub API key is set in configuration
- [ ] CORS policy updated to include production frontend URL
- [ ] SSL/TLS certificate configured (HTTPS)
- [ ] Environment variables set correctly
- [ ] Logging and monitoring configured
- [ ] Error handling returns appropriate status codes
- [ ] API is deployed and accessible
- [ ] Health check endpoint working (if available)

### React Frontend

- [ ] `.env` file created with production API URL
- [ ] API base URL points to production backend
- [ ] Environment variables prefixed with `VITE_`
- [ ] All API endpoints tested against production backend
- [ ] Build completes without errors: `npm run build`
- [ ] Production build tested locally
- [ ] No sensitive data in client-side code
- [ ] Console logs reviewed and cleaned
- [ ] 404 page/route configured

## Deployment

### Backend Deployment

- [ ] Deploy .NET API to hosting service:
  - [ ] Azure App Service
  - [ ] AWS Elastic Beanstalk
  - [ ] Digital Ocean
  - [ ] Self-hosted server
  - [ ] Other: ___________
- [ ] Note deployed API URL: ___________
- [ ] Test API endpoints:
  - [ ] `GET /api/Trade/GetTradeInfo/MSFT`
  - [ ] `POST /api/Trade/BuyOrder`
  - [ ] `POST /api/Trade/SellOrder`
  - [ ] `GET /api/Trade/GetAllBuyOrders`
  - [ ] `GET /api/Trade/GetAllSellOrders`

### Frontend Deployment

- [ ] Update `.env` with production API URL
- [ ] Run production build: `npm run build`
- [ ] Test build locally: `npx serve dist`
- [ ] Deploy to hosting service:
  - [ ] Vercel
  - [ ] Netlify
  - [ ] AWS S3 + CloudFront
  - [ ] Azure Static Web Apps
  - [ ] Other: ___________
- [ ] Note deployed frontend URL: ___________

## Post-Deployment Testing

### Smoke Tests

- [ ] Frontend loads without errors
- [ ] API calls succeed (check Network tab)
- [ ] Default stock (MSFT) loads
- [ ] Can change stock symbols
- [ ] Can create buy orders
- [ ] Can create sell orders
- [ ] Orders page displays correctly
- [ ] PDF export works
- [ ] WebSocket connects (during market hours)
- [ ] Responsive design works on mobile
- [ ] No console errors

### Security Checks

- [ ] API requires HTTPS
- [ ] Frontend served over HTTPS
- [ ] CORS only allows your frontend domain
- [ ] No API keys exposed in client code
- [ ] No sensitive data in browser storage
- [ ] API rate limiting configured
- [ ] Input validation working on backend

### Performance

- [ ] Initial page load < 3 seconds
- [ ] API response times acceptable
- [ ] Images optimized
- [ ] Bundle size reasonable (check with `npm run build`)
- [ ] CDN configured (if applicable)

## Configuration

### Environment Variables

**Frontend** (`.env`):
```
VITE_API_BASE_URL=https://your-api-domain.com
```

**Backend** (appsettings.json or environment):
```json
{
  "FinnhubApiKey": "your-key-here",
  "AllowedOrigins": ["https://your-frontend-domain.com"],
  "ConnectionStrings": {
    "DefaultConnection": "your-connection-string"
  }
}
```

### CORS Configuration

Update your .NET API `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://your-frontend-domain.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

## Monitoring

### Set Up Monitoring For:

- [ ] API uptime monitoring
- [ ] Error tracking (Sentry, Application Insights, etc.)
- [ ] Performance monitoring
- [ ] Database connection health
- [ ] API request logs
- [ ] Failed order creation alerts

### Metrics to Track:

- [ ] API response times
- [ ] Error rates
- [ ] Number of orders created
- [ ] Active users
- [ ] Page load times
- [ ] WebSocket connection stability

## Rollback Plan

In case of issues:

1. [ ] Keep previous deployment artifacts
2. [ ] Document rollback procedure:
   - [ ] Frontend: Revert to previous deployment
   - [ ] Backend: Revert to previous version
   - [ ] Database: Have backup ready
3. [ ] Test rollback in staging environment
4. [ ] Communication plan for users

## DNS and SSL

- [ ] Domain configured for frontend
- [ ] Domain configured for backend (if using custom domain)
- [ ] SSL certificates installed and valid
- [ ] DNS propagation complete
- [ ] Redirects configured (www to non-www, http to https)

## Documentation

- [ ] API documentation updated
- [ ] README.md updated with deployment info
- [ ] Environment variable documentation complete
- [ ] Team notified of deployment
- [ ] Deployment notes documented

## Staging Environment (Recommended)

Before production deployment:

- [ ] Deploy to staging environment
- [ ] Run full test suite
- [ ] Perform manual testing
- [ ] Load testing (if applicable)
- [ ] Get stakeholder approval

## Post-Launch

### First 24 Hours

- [ ] Monitor error logs closely
- [ ] Check API usage
- [ ] Verify WebSocket connections during market hours
- [ ] Test order creation
- [ ] Check database for correct order storage
- [ ] Review user feedback

### First Week

- [ ] Review performance metrics
- [ ] Optimize based on real usage
- [ ] Address any reported issues
- [ ] Document lessons learned

## Support

- [ ] Support email/contact configured
- [ ] Error messages provide helpful information
- [ ] FAQ or help documentation available
- [ ] Team trained on troubleshooting common issues

---

## Quick Reference

**Frontend URL**: ___________
**Backend URL**: ___________
**Database**: ___________
**Deployment Date**: ___________
**Deployed By**: ___________

## Notes

_Add any deployment-specific notes here:_

---

**Status**: 
- [ ] Pending
- [ ] In Progress  
- [ ] Completed
- [ ] Issues (describe): ___________
