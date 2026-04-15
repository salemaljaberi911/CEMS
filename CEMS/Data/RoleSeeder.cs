using CEMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CEMS.Data
{
    public static class RoleSeeder
    {
        private const string StudentRole = "Student";
        private const string OrganizerRole = "Organizer";
        private const string StaffCoordinatorRole = "StaffCoordinator";

        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { StudentRole, OrganizerRole, StaffCoordinatorRole };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            string staffEmail = "salem.aljabri11@gmail.com";
            string organizerEmail = "aljabrisalem99@gmail.com";

            var staffUser = await userManager.FindByEmailAsync(staffEmail);
            if (staffUser != null)
            {
                await EnsureSingleRoleAsync(userManager, staffUser, StaffCoordinatorRole);
            }

            var organizerUser = await userManager.FindByEmailAsync(organizerEmail);
            if (organizerUser != null)
            {
                await EnsureSingleRoleAsync(userManager, organizerUser, OrganizerRole);
            }
        }

        private static async Task EnsureSingleRoleAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationUser user,
            string targetRole)
        {
            var currentRoles = await userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                await userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            if (!await userManager.IsInRoleAsync(user, targetRole))
            {
                await userManager.AddToRoleAsync(user, targetRole);
            }
        }
    }
}