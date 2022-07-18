using Identity_API_Demo.Entity;
using Identity_API_Demo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity_API_Demo.Controllers
{
    [Route("api/SendMail")]
    [Authorize]
    [ApiController]
    public class SendMailSMTP_APIController : ControllerBase
    {
        #region Init
        public IAuthenService _authenService;
        public RoleManager<IdentityRole> _roleManager;      // Use for set role.
        public UserManager<Customer> _userManager;          // Use for register or manager account.
        public SignInManager<Customer> _signInManager;      // Use for login.
        public IConfiguration _configuration;
        public ISendMailService _sendMailService;
        #endregion

        #region Constructor
        public SendMailSMTP_APIController
            (

        // Inject service form constructor.
        IAuthenService authenService,
        RoleManager<IdentityRole> roleManager,
        UserManager<Customer> userManager,
        SignInManager<Customer> signInManager,
        IConfiguration configuration,
        ISendMailService sendMailService

            )
        {
            _authenService = authenService;
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _sendMailService = sendMailService;
        }
        #endregion

        [HttpPost]
        [Route("")]
        public IActionResult SendMail(string to, string from, string subject, string body)
        {
            return Ok(_sendMailService.SendMail(to,from,subject,body));
        }



    }
}
