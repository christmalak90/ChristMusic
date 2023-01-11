using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ChristMusic.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace WebApplication1.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IUnitOfWork _unitOfWork;

        public ResendEmailConfirmationModel(UserManager<IdentityUser> userManager, IEmailSender emailSender, IWebHostEnvironment hostEnvironment, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _hostEnvironment = hostEnvironment;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                TempData["Error"] = "Failed to send Verification email";
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var applicationUser = _unitOfWork.User.GetWithStringId(userId);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);

            #region Send Email

            //Get email html template from wwwroot=>Template=>EmailTemplates=>Confirm_Account_Registration.html to send Email
            var HtmlTemplatePath = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                    + "Templates" + Path.DirectorySeparatorChar.ToString() + "EmailTemplates"
                    + Path.DirectorySeparatorChar.ToString() + "Confirm_Account_Registration.html";

            var subject = "Confirm Email";
            var to = user.Email;
            var name = applicationUser.Name;
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

            return Page();
        }
    }
}
