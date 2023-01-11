using ChristMusic.DataAccess.Data;
using ChristMusic.DataAccess.Repository.IRepository;
using ChristMusic.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChristMusic.DataAccess.Repository
{
    public class UserRoleRepository : Repository<IdentityUserRole<string>>, IUserRoleRepository
    {
        private readonly ApplicationDbContext _db;

        public UserRoleRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        //This table must not be updated
        public void Update(IdentityUserRole<string> identityUserRole)
        {
            //var objFromDb = _db.UserRoles.FirstOrDefault(s=>s.RoleId == identityUserRole.RoleId);

            //if (objFromDb != null)
            //{
            //    objFromDb.UserId = identityUserRole.UserId;
            //}
        }
    }
}
