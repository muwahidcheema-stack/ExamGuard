using ExamGuard.Models;
using Microsoft.AspNetCore.Identity;

namespace ExamGuard.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndDataAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Seed Roles
            string[] roles = { "Admin", "Faculty" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Default Admin
            var adminEmail = "admin@pucit.edu.pk";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Department Admin",
                    Department = "Software Engineering",
                    EmployeeOrRollNumber = "EMP-001",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // 3. Seed Sample TAs
            var ta1Email = "bsef24m040@pucit.edu.pk";
            var ta1 = await userManager.FindByEmailAsync(ta1Email);
            if (ta1 == null)
            {
                ta1 = new ApplicationUser
                {
                    UserName = ta1Email,
                    Email = ta1Email,
                    FullName = "Muhammad Muwahid Cheema",
                    Department = "Software Engineering",
                    EmployeeOrRollNumber = "BSEF24M040",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(ta1, "User@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(ta1, "Faculty");
                }
            }

            var ta2Email = "Muneeb@pucit.edu.pk";
            var ta2 = await userManager.FindByEmailAsync(ta2Email);
            if (ta2 == null)
            {
                ta2 = new ApplicationUser
                {
                    UserName = ta2Email,
                    Email = ta2Email,
                    FullName = "Muneeb Ahmad",
                    Department = "Software Engineering",
                    EmployeeOrRollNumber = "BSEF24M012",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(ta2, "User@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(ta2, "Faculty");
                }
            }

            // 4. Seed Sample Exams & Duties if empty
            if (!context.Exams.Any() && ta1 != null && ta2 != null)
            {
                var exam1 = new Exam
                {
                    CourseCode = "CS301",
                    CourseName = "Web Technologies",
                    ExamDate = DateTime.Today.AddDays(3),
                    StartTime = "09:00 AM",
                    EndTime = "11:30 AM",
                    Venue = "Lab 2",
                    IsPublished = true
                };
                var exam2 = new Exam
                {
                    CourseCode = "SE302",
                    CourseName = "Software Architecture",
                    ExamDate = DateTime.Today.AddDays(5),
                    StartTime = "01:30 PM",
                    EndTime = "04:00 PM",
                    Venue = "Room 104",
                    IsPublished = true
                };

                context.Exams.AddRange(exam1, exam2);
                await context.SaveChangesAsync();

                context.InvigilationDuties.AddRange(
                    new InvigilationDuty { ExamId = exam1.ExamId, AssignedUserId = ta1.Id, Status = "Assigned" },
                    new InvigilationDuty { ExamId = exam2.ExamId, AssignedUserId = ta2.Id, Status = "Assigned" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}