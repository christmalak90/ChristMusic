using ChristMusic.DataAccess.Repository.IRepository;
using ChristMusic.Models;
using ChristMusic.Models.ViewModels;
using ChristMusic.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChristMusic.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            

            //Find the identity of the user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim != null)
            {
                //Store the number of items that the user has in the shopping cart in a session
                var count = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value).ToList().Count(); //claim.Value contains the id of the current user
                HttpContext.Session.SetObject(SD.NbrOfProductInShoppingCartSession, count);
            }

            return View(productList);
        }

        public IActionResult ProductDetails(int id)
        {
            var productFromDb = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id, includeProperties: "Category,CoverType");
            ShoppingCart cartObj = new ShoppingCart()
            {
                Product = productFromDb,
                ProductId = productFromDb.Id
            };
            
            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult ProductDetails(ShoppingCart CartObject)
        {
            //NB. Each combinaison of userId, ProductId and Count has they own record in the shopping Cart table in the database
            
            CartObject.Id = 0;
            if(ModelState.IsValid)
            {
                //Then we will add to cart
                //Find the identity of the user
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.ApplicationUserId == CartObject.ApplicationUserId && u.ProductId == CartObject.ProductId, includeProperties:"Product");
                
                if(cartFromDb == null)
                {
                    //NB. Each combinaison of userId, ProductId and Count has they own record in the shopping Cart table in the database
                    //No records exists in the database for that product for that user
                    _unitOfWork.ShoppingCart.Add(CartObject);
                }
                else
                {
                    //If the records exist, increase the count field
                    cartFromDb.Count += CartObject.Count;
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
                }
                _unitOfWork.Save();

                //Store the number of items that the user has in the shopping cart in a session
                var count = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == CartObject.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetObject(SD.NbrOfProductInShoppingCartSession, count);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productFromDb = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == CartObject.ProductId, includeProperties: "Category,CoverType");
                ShoppingCart cartObj = new ShoppingCart()
                {
                    Product = productFromDb,
                    ProductId = productFromDb.Id
                };
                return View(cartObj);
            } 
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
