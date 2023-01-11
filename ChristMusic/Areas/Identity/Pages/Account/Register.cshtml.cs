using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ChristMusic.DataAccess.Repository.IRepository;
using ChristMusic.Models;
using ChristMusic.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace WebApplication1.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager; //Manages Users in the database
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager; //Manages Roles in the database
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        //Acts like our view model
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Name { get; set; }
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public string PhoneNumber { get; set; }
            [Display(Name="Company")]
            public int? CompanyId { get; set; }
            public string Role { get; set; }

            public IEnumerable<SelectListItem> CompanyList { get; set; }
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            #region Send the list of companies and roles to the form

            Input = new InputModel()
            {
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                RoleList = _roleManager.Roles.Where(u=>u.Name != SD.Role_IndividualCustomer).Select(x=>x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };

            if(User.IsInRole(SD.Role_Employee))
            {
                Input.RoleList = _roleManager.Roles.Where(u => u.Name == SD.Role_CompanyCustomer).Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                });
            }

            #endregion

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Name = Input.Name,
                    Email = Input.Email,
                    StreetAddress = Input.StreetAddress,
                    City = Input.City,
                    State = Input.State,
                    PostalCode = Input.PostalCode,
                    PhoneNumber = Input.PhoneNumber,
                    Role = Input.Role,
                    CompanyId = Input.CompanyId
                };

                //Create user in the database
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if(user.Role == null)
                    {
                        //Assign individual customer role to user
                        await _userManager.AddToRoleAsync(user, SD.Role_IndividualCustomer);
                    }
                    else
                    {
                        //if he is a company customer
                        if(user.CompanyId > 0)
                        {
                            await _userManager.AddToRoleAsync(user, SD.Role_CompanyCustomer);
                        }
                        else
                        {
                            await _userManager.AddToRoleAsync(user, user.Role);
                        }
                    }

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
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
                    
                    #endregion

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        //if user registers from the website. user.Role is null because a user cannot assign a role to himself, only admin assigns a role to a user
                        if (user.Role == null)
                        {
                            //Log in the user
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                        //if admin registers a new user
                        else
                        {
                            return RedirectToAction("Index","User", new { Area="Admin" });
                        }
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form

            #region Send the list of companies and roles to the form

            Input = new InputModel()
            {
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                RoleList = _roleManager.Roles.Where(u => u.Name != SD.Role_IndividualCustomer).Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };

            #endregion

            return Page();
        }
    }
}
