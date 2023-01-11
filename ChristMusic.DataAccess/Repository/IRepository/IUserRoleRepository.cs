using ChristMusic.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChristMusic.DataAccess.Repository.IRepository
{
    public interface IUserRoleRepository : IRepository<IdentityUserRole<string>>
    {
        void Update(IdentityUserRole<string> identityUserRole);
    }
}
