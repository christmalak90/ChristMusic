using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ChristMusic.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace ChristMusic.ViewComponents
{
    public class UserNameViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserNameViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userFromDb = _unitOfWork.User.GetFirstOrDefault(u => u.Id == claims.Value);

            return View(userFromDb);
        }
    }
}
