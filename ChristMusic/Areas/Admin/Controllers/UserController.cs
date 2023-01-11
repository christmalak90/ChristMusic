using ChristMusic.DataAccess.Repository.IRepository;
using ChristMusic.Models;
using ChristMusic.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChristMusic.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS

        //Retrieve all the categories and return in a Json format
        [HttpGet]
        public IActionResult GetAll()
        {
            //get list of users
            var userList = _unitOfWork.User.GetAll(includeProperties: "Company");
            //database table mapping between usersId and rolesId
            var userRole = _unitOfWork.UserRole.GetAll();
            //list of roles in the database
            var roles = _unitOfWork.Role.GetAll();

            foreach(var user in userList)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                
                //if the "Company" navigation property of the user is null, initialise a new company to avoid errors in the view
                if(user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            return Json(new { data = userList });
        }

        //Lock or Unlock user
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _unitOfWork.User.GetWithStringId(id);
            if(objFromDb == null)
            {
                return Json(new { success=false, message="Error while Locking/Unlocking" });
            }
            if(objFromDb.LockoutEnd!=null && objFromDb.LockoutEnd > DateTime.Now) //lockoutEnd is a field in the AspNetUsers table in the database that specifies the time in which a user is locked
            {
                //User is in Locked state

                //code to unlock the user
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                //User is not locked

                //code to lock the user for 1 year
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1);
            }
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #endregion
    }
}
