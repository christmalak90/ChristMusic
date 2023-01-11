using ChristMusic.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChristMusic.DataAccess.Repository.IRepository
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        void Update(ApplicationUser applicationUser);
    }
}
