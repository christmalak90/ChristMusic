using ChristMusic.DataAccess.Repository.IRepository;
using ChristMusic.Models;
using ChristMusic.Models.ViewModels;
using ChristMusic.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChristMusic.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        //We need IWebHostEnvironment because we will be uploading images on the server in a folder inside wwwroot
        //IWebHostEnvironment will help us to get the absolute path of the images
        private readonly IWebHostEnvironment _hostEnvironment;
                
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id) //Id is given in case of Edit to know the product to be edited from the id
        {
            IEnumerable<Category> CatList = await _unitOfWork.Category.GetAllAsync();
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = CatList.Select(i => new SelectListItem {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            if(id == null)
            {
                //this is for create
                return View(productVM);
            }

            //this is for edit
            productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
            if(productVM.Product == null)
            {
                return NotFound();
            }
            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //prevent cross-side request forgery attack
        public async Task<IActionResult> Upsert(ProductVM productVM)
        {
            if(ModelState.IsValid)
            {
                //Get the path of the wwwroot folder
                string webRootPath = _hostEnvironment.WebRootPath;
                //Retrieve all the files to be uploaded
                var files = HttpContext.Request.Form.Files;

                if(files.Count > 0) 
                {
                    string fileName = Guid.NewGuid().ToString();

                    //Get the path from the wwwroot folder to the products folder
                    var uploads = Path.Combine(webRootPath, @"images\products");
                    //Get the extension of the file to be uploaded
                    var extension = Path.GetExtension(files[0].FileName);

                    if(productVM.Product.ImageUrl != null)
                    {
                        //This is an edit and we need to remove old image
                        //Get the path of old image
                        var oldImagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    //Upload new file on the server
                    using (var filesStreams = new FileStream(Path.Combine(uploads, fileName+extension),FileMode.Create))
                    {
                        files[0].CopyTo(filesStreams);
                    }

                    productVM.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }
                else
                {
                    if(productVM.Product.Id != 0)
                    {
                        //Edit when they did not change the image
                        Product objFromDb = _unitOfWork.Product.Get(productVM.Product.Id);
                        productVM.Product.ImageUrl = objFromDb.ImageUrl;
                    }
                }

                if (productVM.Product.Id == 0)
                {
                    //For Create
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    //For Edit
                    _unitOfWork.Product.Update(productVM.Product);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                IEnumerable<Category> CatList = await _unitOfWork.Category.GetAllAsync();
                productVM.CategoryList = CatList.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                productVM.CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });

                if(productVM.Product.Id != 0)
                {
                    productVM.Product = _unitOfWork.Product.Get(productVM.Product.Id);
                }
            }
            return View(productVM);
        }

        #region API CALLS

        //Retrieve all the categories and return in a Json format
        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Product.Get(id);
            if(objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string webRootPath = _hostEnvironment.WebRootPath;
            var oldImagePath = Path.Combine(webRootPath, objFromDb.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
