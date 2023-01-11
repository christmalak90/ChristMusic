using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChristMusic.DataAccess.Repository.IRepository;
using ChristMusic.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace WebApplication1.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private TwilioOptions _twilioOptions { get; set; }

        public ConfirmEmailModel(UserManager<IdentityUser> userManager, IOptions<TwilioOptions> twilioOptions, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _twilioOptions = twilioOptions.Value;
            _unitOfWork = unitOfWork;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";

            //Send SMS
            var _user = _unitOfWork.User.GetWithStringId(userId);
            TwilioClient.Init(_twilioOptions.AccountSID, _twilioOptions.AuthToken);

            try
            {
                var message = MessageResource.Create(
                    body: "Dear " + _user.Name + " thank you for Confirming your Email",
                    from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
                    to: new Twilio.Types.PhoneNumber(_user.PhoneNumber)
                    );
            }
            catch (Exception ex)
            {

            }

            return Page();
        }
    }
}
