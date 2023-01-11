using ChristMusic.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChristMusic.DataAccess.Repository.IRepository
{
    public interface IRoleRepository : IRepository<IdentityRole>
    {
        void Update(IdentityRole identityRole);
    }
}
