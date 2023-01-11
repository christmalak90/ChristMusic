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
    //Represent the list of roles in the database
    public class RoleRepository : Repository<IdentityRole>, IRoleRepository
    {
        private readonly ApplicationDbContext _db;

        public RoleRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(IdentityRole role)
        {
            var objFromDb = _db.Roles.FirstOrDefault(s=>s.Id == role.Id);

            if (objFromDb != null)
            {
                objFromDb.Name = role.Name;
            }
        }
    }
}
