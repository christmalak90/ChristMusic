using Braintree;
using ChristMusic.DataAccess.Repository.IRepository;
using ChristMusic.Models;
using ChristMusic.Models.ViewModels;
using ChristMusic.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ChristMusic.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _hostEnvironment;
        private TwilioOptions _twilioOptions { get; set; }
        private readonly UserManager<IdentityUser> _userManager;
        public IBrainTreeGate _brainTreeGate { get; set; }

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public ShoppingCartController(IUnitOfWork unitOfWork, IEmailSender emailSender, IOptions<TwilioOptions> twilioOptions, UserManager<IdentityUser> userManager, IBrainTreeGate brainTreeGate, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _twilioOptions = twilioOptions.Value;
            _userManager = userManager;
            _brainTreeGate = brainTreeGate;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            //Get the identity of the user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                //General Information when the user place the order from the shopping cart 
                OrderHeader = new OrderHeader(),
                //Get all the products that the user has inserted in the shopping cart
                ListOfShoppingCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product") //claim.Value contains the Id of the user
            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.User.GetFirstOrDefault(u => u.Id == claim.Value, includeProperties: "Company");

            foreach (var cart in ShoppingCartVM.ListOfShoppingCart)
            {
                cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                cart.Product.Description = SD.ConvertToRawHtml(cart.Product.Description);
                if (cart.Product.Description.Length > 100)
                {
                    cart.Product.Description = cart.Product.Description.Substring(0, 99) + "...";
                }
            }

            return View(ShoppingCartVM);
        }

        //This Post Action is used by the user to resend the email confirmation message
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            //Get the identity of the user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.User.GetFirstOrDefault(u => u.Id == claim.Value);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email is empty");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            #region Send Email

            //Get email html template from wwwroot=>Template=>EmailTemplates=>Confirm_Account_Registration.html to send Email
            var HtmlTemplatePath = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                    + "Templates" + Path.DirectorySeparatorChar.ToString() + "EmailTemplates"
                    + Path.DirectorySeparatorChar.ToString() + "Confirm_Account_Registration.html";

            var subject = "Confirm Email";
            var to = user.Email;
            var name = user.Name;
            string Message = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";
            string emailBody = "";
            using (StreamReader streamReader = System.IO.File.OpenText(HtmlTemplatePath))
            {
                emailBody = streamReader.ReadToEnd();
            }

            //In the Html Template we have the numbers "{0},{1},{2},{3},{4},{5}"
            //{0} : Subject
            //{1} : DateTime
            //{2} : Name
            //{3} : Email
            //{4} : Message
            //{5} : CallbackURL

            emailBody = string.Format(emailBody, subject, string.Format("{0:dddd, d MMMM yyyy}", DateTime.Now), name, to, Message, callbackUrl);

            await _emailSender.SendEmailAsync(to, subject, emailBody);
            TempData["Success"] = "Verification email sent. Please check your email.";

            #endregion

            return RedirectToAction("Index");
        }

        //BrainTree payment action methode
        [HttpPost]
        public IActionResult BrainTree()
        {
            var gateway = _brainTreeGate.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken; //you can also create a view model
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM
            {
                OrderHeader = new OrderHeader(),
                ListOfShoppingCart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value, includeProperties:"Product")
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.User.GetFirstOrDefault(c => c.Id == claim.Value, includeProperties: "Company");

            foreach (var cart in ShoppingCartVM.ListOfShoppingCart)
            {
                cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                cart.Product.Description = SD.ConvertToRawHtml(cart.Product.Description);
                if (cart.Product.Description.Length > 100)
                {
                    cart.Product.Description = cart.Product.Description.Substring(0, 99) + "...";
                }
            }

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode= ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        //This methode is used to submit the order summary when pressing the place order button
        public IActionResult SummaryPost(string stripeToken, IFormCollection collection) 
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.User.GetFirstOrDefault(c => c.Id == claim.Value, includeProperties: "Company");
            ShoppingCartVM.ListOfShoppingCart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value, includeProperties: "Product");
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus= SD.OrderStatusPending;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach(var item in ShoppingCartVM.ListOfShoppingCart)
            {
                item.Price = SD.GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = item.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = item.Price,
                    Count = item.Count
                };
                ShoppingCartVM.OrderHeader.OrderTotal += (orderDetails.Price * orderDetails.Count);
                _unitOfWork.OrderDetails.Add(orderDetails);
            }

            //Then clear the shopping cart of the user and update the "Nbr of item in cart" session
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListOfShoppingCart);
            _unitOfWork.Save();
            HttpContext.Session.SetObject(SD.NbrOfProductInShoppingCartSession, 0);

            if (stripeToken == null && !collection.ContainsKey("payment_method_nonce"))
            {
                //Process the payment without stripe. -	Allow an authorized company user to place the order without payment and pay 30 days after the order is delivered 
                ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
            }
            else
            {
                if(stripeToken != null)
                {
                    //Process the payment with stripe
                    var options = new ChargeCreateOptions
                    {
                        Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal * 100),
                        Currency = "usd",
                        Description = "Order ID : " + ShoppingCartVM.OrderHeader.Id,
                        Source = stripeToken
                    };
                    var service = new ChargeService();
                    Charge charge = service.Create(options);

                    if (charge.Id == null)
                    {
                        //there is some issue with the payment
                        ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                        TempData["Error"] = "Transaction Failed";
                    }
                    else
                    {
                        ShoppingCartVM.OrderHeader.TransactionId = charge.Id;
                    }
                    if (charge.Status.ToLower() == "succeeded")
                    {
                        ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                        ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                        ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
                        TempData["Success"] = "Transaction was successful. Transaction ID: " + charge.Id + ", Amount charged: $" + options.Amount;
                    }
                }

                if(collection.ContainsKey("payment_method_nonce"))
                {
                    //Payment with BrainTree
                    string nonceFromClient = collection["payment_method_nonce"];
                    var request = new TransactionRequest
                    {
                        Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal),
                        PaymentMethodNonce = nonceFromClient,
                        OrderId = ShoppingCartVM.OrderHeader.Id.ToString(),
                        Options = new TransactionOptionsRequest
                        {
                            SubmitForSettlement = true
                        }
                    };

                    var gateway = _brainTreeGate.GetGateway();
                    Result<Transaction> result = gateway.Transaction.Sale(request);

                    if (result.Target.ProcessorResponseText == "Approved")
                    {
                        ShoppingCartVM.OrderHeader.TransactionId = result.Target.Id;
                        ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                        ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                        ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
                        TempData["Success"] = "Transaction was successful. Transaction ID: " + result.Target.Id + ", Amount charged: $" + result.Target.Amount;
                    }
                    else
                    {
                        ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                        TempData["Error"] = "Transaction Failed";
                    }
                }
            }

            _unitOfWork.Save();
            return RedirectToAction("OrderConfirmation", "ShoppingCart", new { orderHeaderId = ShoppingCartVM.OrderHeader.Id });
        }

        //Action called after the Order is placed
        public IActionResult OrderConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id == orderHeaderId);
            TwilioClient.Init(_twilioOptions.AccountSID, _twilioOptions.AuthToken);

            try
            {
                var message = MessageResource.Create(
                    body: "Your payment have been approved on ChristMusic and your Order successfully Placed . Your Order ID is: " + orderHeaderId,
                    from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
                    to: new Twilio.Types.PhoneNumber(orderHeader.PhoneNumber)
                    );
            }
            catch(Exception ex)
            {

            }

            return View(orderHeaderId);
        }

        //Increment product in shopping cart
        public IActionResult IncrementProduct(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId, includeProperties: "Product");
            cart.Count += 1;
            //Recalculate the price based on quantity
            cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        //Decrement product in shopping cart
        public async Task<IActionResult> DecrementProduct(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId, includeProperties: "Product");
            if (cart.Count == 1)
            {
                //Get nbr of products in shoping cart
                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();

                //Update the Nbr of products in shopping Cart session
                HttpContext.Session.SetObject(SD.NbrOfProductInShoppingCartSession, count - 1);
            }
            else
            {
                cart.Count -= 1;
                //Recalculate the price based on quantity
                cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        //Remove product in shopping cart
        public async Task<IActionResult> RemoveProduct(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId, includeProperties: "Product");
            //Get nbr of products in shoping cart    
            var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();

            //Update the Nbr of products in shopping Cart session
            HttpContext.Session.SetObject(SD.NbrOfProductInShoppingCartSession, count - 1);

            return RedirectToAction(nameof(Index));
        }
    }
}
