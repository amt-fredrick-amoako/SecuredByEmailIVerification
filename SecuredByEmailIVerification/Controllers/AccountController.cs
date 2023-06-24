using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecuredByEmailIVerification.Model;
using SecuredByEmailIVerification.ServiceContracts;
using System.Security.Claims;

namespace SecuredByEmailIVerification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMailService _mailService;
        private readonly IJwtService _jwtService;


        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IMailService mailService, IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _mailService = mailService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApplicationUser>> PostRegister(RegisterDTO registerDTO)
        {
            // validation
            if (!ModelState.IsValid)
            {
                string errorMessage = string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Problem(errorMessage);
            }

            // create user
            ApplicationUser user = new ApplicationUser
            {
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber,
                UserName = registerDTO.Email,
                PersonName = registerDTO.PersonName
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (result.Succeeded)
            {
                //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                //var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, email = user.Email }, Request.Scheme);
                //MailRequest mailRequest = new MailRequest
                //{
                //    Subject = "Confirm Email",
                //    Body = $"Dear customer please confirm your email with this confirmation link: {confirmationLink}",
                //    ToEmail = user.Email,
                //};
                //await _mailService.SendEmailAsync(mailRequest);
                // sign in
                await _signInManager.SignInAsync(user, isPersistent: false);
                AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);

                return Ok(authenticationResponse);
            }
            else
            {
                string error = string.Join("|", result.Errors.Select(e => e.Description));
                return Problem(error);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO login)
        {
            // validation
            if (!ModelState.IsValid)
            {
                string errorMessage = string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Problem(errorMessage);
            }

            if (login.Provider == "Google")
            {
                var redirectUri = Url.Action(nameof(GoogleLoginCallback), "Account", null, Request.Scheme);
                var properties = new AuthenticationProperties { RedirectUri = redirectUri };
                return Challenge(properties, GoogleDefaults.AuthenticationScheme);
            }

            ApplicationUser? user = await _userManager.FindByEmailAsync(login.Email); // find by email

            if (user == null)
            {
                return Problem("Incorrect Email or Password"); // return problem if null
            }

            //bool IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user); 

            if (!user.EmailConfirmed) // check email confirmation
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, email = user.Email }, Request.Scheme);

                MailRequest mailRequest = new MailRequest
                {
                    Subject = "Confirm Email",
                    Body = $"Dear customer please confirm your email with this confirmation link: {confirmationLink}",
                    ToEmail = user.Email,
                };
                await _mailService.SendEmailAsync(mailRequest);
                return Redirect(nameof(AccountController));
            } // send confirmation link if not confirmed

            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);

            return Ok(authenticationResponse);

        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token, [FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("Not found");
            }
            var confirmedToken = token;
            string decodedConfirmToken = System.Web.HttpUtility.UrlDecode(confirmedToken);
            var result = await _userManager.ConfirmEmailAsync(user, decodedConfirmToken);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok("Email has been confirmed successfully");
            }
            else
            {
                return Problem(nameof(ConfirmEmail), "Error");
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GoogleLoginCallback()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                // Handle authentication failure
                return Problem("Google authentication failed");
            }

            // Extract user information from the authentication result
            string email = result.Principal.FindFirstValue(ClaimTypes.Email);
            string name = result.Principal.FindFirstValue(ClaimTypes.Name);

            // Check if the user exists in your application's database
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // User doesn't exist, create a new user account
                user = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    PersonName = name
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    // Failed to create user account
                    string error = string.Join("|", createResult.Errors.Select(e => e.Description));
                    return Problem(error);
                }
            }

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Generate JWT token
            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);

            return Ok(authenticationResponse);
        }

    }
}
