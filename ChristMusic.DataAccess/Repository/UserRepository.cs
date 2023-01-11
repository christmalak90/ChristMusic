using ChristMusic.DataAccess.Data;
using ChristMusic.DataAccess.Repository.IRepository;
using ChristMusic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChristMusic.DataAccess.Repository
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public UserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ApplicationUser applicationUser)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(s=>s.Id == applicationUser.Id);

            if (objFromDb != null)
            {
                objFromDb.Name = applicationUser.Name;
            }
        }
    }
}
