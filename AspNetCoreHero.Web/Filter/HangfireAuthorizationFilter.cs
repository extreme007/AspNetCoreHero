using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreHero.Web.Filter
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private string _role;

        public HangfireAuthorizationFilter(string role = null)
        {
            _role = role;
        }
        public bool Authorize(DashboardContext context)
        {
            //#if DEBUG
            //            // If we are in debug, always allow Hangfire access.
            //            return true;
            //#else
            //            var httpContext = context.GetHttpContext();
            //            var accessTokenHangfire = httpContext.Request.Cookies["accessTokenHangfire"];
            //            if (!string.IsNullOrEmpty(accessTokenHangfire))
            //            {
            //                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            //                JwtSecurityToken securityToken = handler.ReadToken(accessTokenHangfire) as JwtSecurityToken;
            //                bool result = securityToken.Claims.Where(claim => claim.Type == "roles").Select(x=>x.Value).Contains(_role);
            //                return result;                
            //            }
            //            return false;            
            //#endif


            var httpContext = context.GetHttpContext();
            var accessTokenHangfire = httpContext.Request.Cookies["accessTokenHangfire"];
            if (!string.IsNullOrEmpty(accessTokenHangfire))
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken securityToken = handler.ReadToken(accessTokenHangfire) as JwtSecurityToken;
                bool result = securityToken.Claims.Where(claim => claim.Type == "roles").Select(x => x.Value).Contains(_role);
                return result;
            }
            return false;
        }
    }
}
