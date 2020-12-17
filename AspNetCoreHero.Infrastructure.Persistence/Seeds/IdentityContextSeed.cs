using AspNetCoreHero.Application.Constants;
using AspNetCoreHero.Application.Constants.Permissions;
using AspNetCoreHero.Application.Enums.Identity;
using AspNetCoreHero.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Infrastructure.Persistence.Seeds
{
    public static class IdentityContextSeed
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var listRoles = Enum.GetValues(typeof(Roles)).Cast<Roles>().Select(r=>r.ToString());
            //Seed Roles
            foreach(var role in listRoles)
            {
                var existed = await roleManager.RoleExistsAsync(role);
                if (!existed)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            if(userManager.FindByEmailAsync("superadmin@gmail.com").Result == null)
            {
                //Seed Default SuperAdmin
                var defaultSuperAdmin = new ApplicationUser
                {
                    UserName = "superadmin",
                    Email = "superadmin@gmail.com",
                    FirstName = "Super",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true
                };
                IdentityResult result = userManager.CreateAsync(defaultSuperAdmin, "123Pa$$word!").Result;
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultSuperAdmin, Roles.Basic.ToString());
                    await userManager.AddToRoleAsync(defaultSuperAdmin, Roles.Moderator.ToString());
                    await userManager.AddToRoleAsync(defaultSuperAdmin, Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(defaultSuperAdmin, Roles.SuperAdmin.ToString());
                }
            }

            if (userManager.FindByEmailAsync("basicuser@gmail.com").Result == null)
            {
                //Seed Default User
                var defaultUser = new ApplicationUser
                {
                    UserName = "basicuser",
                    Email = "basicuser@gmail.com",
                    FirstName = "Basic",
                    LastName = "User",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true
                };
                IdentityResult result = userManager.CreateAsync(defaultUser, "123Pa$$word!").Result;
                if(result.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser, Roles.Basic.ToString());
                }            
            }
        }

        public static async Task SeedDataAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            await SeedClaimsForSuperAdmin(roleManager);
        }

        private async static Task SeedClaimsForSuperAdmin(this RoleManager<IdentityRole> roleManager)
        {
            var adminRole = await roleManager.FindByNameAsync("SuperAdmin");
            await roleManager.AddPermissionClaim(adminRole, MasterPermissions.Create);
            await roleManager.AddPermissionClaim(adminRole, MasterPermissions.Update);
            await roleManager.AddPermissionClaim(adminRole, MasterPermissions.View);
            await roleManager.AddPermissionClaim(adminRole, MasterPermissions.Delete);
        }
        public static async Task AddPermissionClaim(this RoleManager<IdentityRole> roleManager, IdentityRole role, string permission)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
            {
                await roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
            }
        }
        public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUser = new ApplicationUser
            {
                UserName = "superadmin",
                Email = "superadmin@gmail.com",
                FirstName = "Super",
                LastName = "Admin",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "123Pa$$word!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Basic.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.Moderator.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.SuperAdmin.ToString());
                }
                else
                {
                    await userManager.AddToRoleAsync(user, Roles.SuperAdmin.ToString());
                }
                
            }
        }
        public static async Task SeedBasicUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUser = new ApplicationUser
            {
                UserName = "basicuser",
                Email = "basicuser@gmail.com",
                FirstName = "Basic",
                LastName = "User",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "123Pa$$word!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Basic.ToString());
                }

            }
        }
    }
}
