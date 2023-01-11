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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id) //Id is given in case of Edit to know the company to be edited from the id
        {
            Company company = new Company();

            if(id == null)
            {
                //this is for create
                return View(company);
            }

            //this is for edit
            company = _unitOfWork.Company.Get(id.GetValueOrDefault());
            if(company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //prevent cross-side request forgery attack
        public IActionResult Upsert(Company company)
        {
            if(ModelState.IsValid)
            {
                if(company.Id == 0)
                {
                    //For Create
                    _unitOfWork.Company.Add(company);
                }
                else
                {
                    //For Edit
                    _unitOfWork.Company.Update(company);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        #region API CALLS

        //Retrieve all the categories and return in a Json format
        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Company.GetAll();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Company.Get(id);
            if(objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Company.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
