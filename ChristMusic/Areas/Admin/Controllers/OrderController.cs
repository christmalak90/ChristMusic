using ChristMusic.DataAccess.Repository.IRepository;
using ChristMusic.Models;
using ChristMusic.Models.ViewModels;
using ChristMusic.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ChristMusic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private TwilioOptions _twilioOptions { get; set; }

        [BindProperty]
        public OrderDetailsVM OrderDetailsVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork, IOptions<TwilioOptions> twilioOptions)
        {
            _unitOfWork = unitOfWork;
            _twilioOptions = twilioOptions.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        //Action to display the details of an order
        public IActionResult OrderDetails(int id) //id is OrderHeaderId
        {
            OrderDetailsVM = new OrderDetailsVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetails.GetAll(o => o.OrderId == id, includeProperties: "Product")
            };

            return View(OrderDetailsVM);
        }

        //Method to receive the delayed payment of authorized company
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("OrderDetails")]
        public IActionResult OrderDetails(string stripeToken)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderDetailsVM.OrderHeader.Id,includeProperties:"ApplicationUser");
            if(stripeToken != null)
            {
                //Process the payment with stripe
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order ID : " + orderHeader.Id,
                    Source = stripeToken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.Id == null)
                {
                    //there is some issue with the payment
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.Id;
                }
                if (charge.Status.ToLower() == "succeeded")
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    orderHeader.PaymentDate = DateTime.Now;
                }
                _unitOfWork.Save();
            }
            return RedirectToAction("OrderDetails", "Order", new { id = orderHeader.Id });
        }

        //Method to process the order
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing(int id) //id is OrderHeaderId
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            orderHeader.OrderStatus = SD.OrderStatusInProcess;
            _unitOfWork.Save();

            //Send SMS
            TwilioClient.Init(_twilioOptions.AccountSID, _twilioOptions.AuthToken);

            try
            {
                var message = MessageResource.Create(
                    body: "Your Order Number: " + orderHeader.Id + " is now in process on ChristMusic",
                    from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
                    to: new Twilio.Types.PhoneNumber(orderHeader.PhoneNumber)
                    );
            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("index");
        }

        //Method to Ship the order
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderDetailsVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderDetailsVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderDetailsVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.OrderStatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _unitOfWork.Save();

            //Send SMS
            TwilioClient.Init(_twilioOptions.AccountSID, _twilioOptions.AuthToken);

            try
            {
                var message = MessageResource.Create(
                    body: "Your Order Number: " + orderHeader.Id + " has been Shipped on ChristMusic. The order tracking number is :" + orderHeader.TrackingNumber + "",
                    from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
                    to: new Twilio.Types.PhoneNumber(orderHeader.PhoneNumber)
                    );
            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("index");
        }

        //Method to cancel the order
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(int id) //id is OrderHeaderId
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            if(orderHeader.PaymentStatus==SD.PaymentStatusApproved)
            {
                //Refund the user
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal*100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TransactionId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                orderHeader.OrderStatus = SD.OrderStatusCancelled;
                orderHeader.PaymentStatus = SD.PaymentStatusRefunded;
            }
            else
            {
                orderHeader.OrderStatus = SD.OrderStatusCancelled;
                orderHeader.PaymentStatus = SD.PaymentStatusCancelled;
            }
            
            _unitOfWork.Save();

            //Send SMS
            TwilioClient.Init(_twilioOptions.AccountSID, _twilioOptions.AuthToken);

            try
            {
                var message = MessageResource.Create(
                    body: "Your Order Number: " + orderHeader.Id + " has been cancelled on ChristMusic",
                    from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
                    to: new Twilio.Types.PhoneNumber(orderHeader.PhoneNumber)
                    );
            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("index");
        }


        #region API CALLS

        //API CALLS to get Order List
        [HttpGet]
        public IActionResult GetOrderList(string orderStatus)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaderList;

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                //if the user is an Admin or an employee, show all the orders of all users
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                //Show only the orders of the user logged in
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }

            switch (orderStatus)
            {
                case "paymentPending":
                    orderHeaderList = orderHeaderList.Where(o=>o.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "orderInprocess":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == SD.OrderStatusApproved || o.OrderStatus==SD.OrderStatusInProcess || o.OrderStatus == SD.OrderStatusPending);
                    break;
                case "orderCompleted":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == SD.OrderStatusShipped);
                    break;
                case "orderRejected":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == SD.OrderStatusCancelled || o.PaymentStatus == SD.PaymentStatusRefunded || o.PaymentStatus == SD.PaymentStatusRejected);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaderList });
        }

        #endregion
    }
}
