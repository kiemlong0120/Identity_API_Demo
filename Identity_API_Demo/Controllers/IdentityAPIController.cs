using Identity_API_Demo.Entity;
using Identity_API_Demo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity_API_Demo.Controllers
{
    [Route("api/Identity")]
    [Authorize]
    [ApiController]
    public class IdentityAPIController : ControllerBase
    {
        #region Init
        private readonly IAuthenService _authenService;
        private readonly RoleManager<IdentityRole> _roleManager;      // Use for set role.
        private readonly UserManager<Customer> _userManager;          // Use for register or manager account.
        private readonly SignInManager<Customer> _signInManager;      // Use for login.
        private readonly IConfiguration _configuration;
        private readonly ISendMailService _sendMailService;
        #endregion

        #region Constructor
        public IdentityAPIController
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

      

        #region API

        #region Register_API

        /// <summary>
        /// API create a new customer with Username and Password require use Register service.
        /// </summary>
        /// <param name="Username">Username from input</param>
        /// <param name="Password">Password from input</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]                     // Allow access by non-authenticated user
        [Route("Register")]
        public async Task<IActionResult> Register(string Username, string Password, string Roles)
        {
            #region Setup_Role

            // Check if role exit, if no then create new role.
            var newRole = await _roleManager.RoleExistsAsync(Roles);
            if (!newRole)
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = Roles });
            }
            #endregion

            // Check if customer ID exit by call service GetCustomer.
            if (_authenService.GetCustomer(Username) == null)
            {
                var newCustomer = new Customer
                {
                    UserName = Username,
                    Name = Username
                };

                // Call Identity service CreateAsync to add new Customer to database.
                IdentityResult result = await _userManager.CreateAsync(newCustomer, Password);
                // In CreateAsync, user password will be enrypt and hash.

                if (result.Succeeded)
                {
                    // If succeed, add role for customer.
                    await _userManager.AddToRoleAsync(newCustomer, Roles);

                    // Call service to set role name
                    _authenService.SetRoleName(newCustomer.Id);
                    return Created(
                                  HttpContext.Request.Scheme +
                                  "://" + HttpContext.Request.Host +
                                  HttpContext.Request.Path +
                                  newCustomer.Id,
                                  newCustomer);
                }
                else
                {
                    // Throw exception of CreateAsync
                    var exceptionText = result.ToString();
                    return BadRequest($"Error: {exceptionText}");
                }
            }
            return BadRequest($"Customer with Username {Username} already exist");
        }
        #endregion

        #region Login_API
        /// <summary>
        /// API Login.
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]         // Allow access by non-authenticated user.
        [Route("Login")]
        public async Task<IActionResult> Login(string Username, string Password)
        {
            // Check if userName exit.
            var user = await _userManager.FindByNameAsync(Username);

            // Check if correct username and password
            if (user != null && await _userManager.CheckPasswordAsync(user, Password))
            {
                // Get user's role to add to JWT
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

                return Ok(new
                {
                    token = ("Bearer ")+ new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return BadRequest("Invalid Username or Password. Please check again");

        }
        #endregion

        #region Get_All_Customer_API
        /// <summary>
        /// API to get all customer by admin only
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Roles = "admin")]        // Limited access by admin
        [AllowAnonymous]
        public IActionResult GetAllUser()
        {
            return Ok(_authenService.GetAllCustomer());
        }
        #endregion

        #region Get_All_Role_API
        /// <summary>
        /// API to get all customer by admin only
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "admin")]
        [Route("Roles")]
        public IActionResult ShowAllRole()
        {
            return Ok(_authenService.ShowAllRole());
        }
        #endregion

        #region Delete_API

        /// <summary>
        /// API delete username by userName
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Roles = "admin")]
        [Route("Delete")]
        public async Task<IActionResult> DeleteUsers(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            _authenService.DeleteUsers(user);
            return Ok("Delete success");
        }

        #endregion

        #region Update_API

        /// <summary>
        /// API delete username by userName
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Roles = "admin")]
        [Route("Update")]
        public async Task<IActionResult> UpdateUsers(string userName, Customer customer)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user!=null)
            {
                return Ok(_authenService.UpdateUser(userName, customer));
            }
            return BadRequest($"Username {userName} does not exit!");
        }

        #endregion

        #region GetbyId_API

        [HttpGet]
        [Authorize(Roles = "admin")]        // Limited access by admin
        [Route("{Username}")]
        public IActionResult GetUserbyUsername(string Username)
        {
            var cus = _authenService.GetCustomerByUserName(Username);
            if (cus!=null)
            {
                return Ok(cus);
            }
            return NotFound("Username not found");

        }
        #endregion


        #endregion

        #region Fuction
        /// <summary>
        /// Generate token, use in Login APi
        /// </summary>
        /// <param name="authClaims"></param>
        /// <returns></returns>
        protected JwtSecurityToken GetToken(List<Claim> authClaims)
        {

            // Generate new key by reading secret key set in appsetting.json
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddMinutes(10),          // Setting lifetime for JWT
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }
        #endregion



    }
}
