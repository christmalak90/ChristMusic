using ChristMusic.DataAccess.Data;
using ChristMusic.Models;
using ChristMusic.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChristMusic.DataAccess.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void Initialize()
        {
            try
            {
                if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate(); //Push all the pending migration to the database automatically
                }
            }
            catch
            {

            }

            if (_db.Roles.Any(r => r.Name == SD.Role_Admin))
            {
                return;
            }

            //Create roles in the database
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_CompanyCustomer)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_IndividualCustomer)).GetAwaiter().GetResult();

            //Create Admin user
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "christMusicLtd@gmail.com",
                Email = "christMusicLtd@gmail.com",
                EmailConfirmed = true,
                Name = "christMusicLtd"
            },"Mc_901006").GetAwaiter().GetResult();

            ApplicationUser user = _db.ApplicationUsers.Where(u => u.Email == "christMusicLtd@gmail.com").FirstOrDefault();

            //Assign Admin role to the user
            _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
        }
    }
}
