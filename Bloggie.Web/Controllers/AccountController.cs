using Bloggie.Web.Data;
using Bloggie.Web.Models.ViewModels;
using Bloggie.Web.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using AspNet.Security.OAuth.GitHub;

namespace Bloggie.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly AuthDbContext authDbContext;

        public AccountController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            AuthDbContext authDbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.authDbContext = authDbContext;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            
            if (ModelState.IsValid)
            {
                var identityUser = new IdentityUser
                {
                    UserName = registerViewModel.Username,
                    Email = registerViewModel.Email
                };

                var identityResult = await userManager.CreateAsync(identityUser, registerViewModel.Password);

                ValidatePassword(registerViewModel.Password);

                if (identityResult.Succeeded)
                {
                    // assign this user the "User" role
                    var roleIdentityResult = await userManager.AddToRoleAsync(identityUser, "User");

                    if (roleIdentityResult.Succeeded)
                    {
                        var signInResult = await signInManager.PasswordSignInAsync(registerViewModel.Username,
                        registerViewModel.Password, false, false);

                        if (signInResult != null && signInResult.Succeeded)
                        {
                            // Show success notification
                            return RedirectToAction("Index", "Home");
                        }

                    }

                }else
                {
                    ModelState.AddModelError("Username", "There is already a user with this username");
                }
            }

            // show error notification
            return View();
        }

        [HttpGet]
        public IActionResult Login(string ReturnUrl)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = ReturnUrl
            };

            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var signInResult = await signInManager.PasswordSignInAsync(loginViewModel.Username,
                loginViewModel.Password, false, false);

            if (signInResult != null && signInResult.Succeeded) 
            {

                if (!string.IsNullOrWhiteSpace(loginViewModel.ReturnUrl))
                {
                    TempData["SuccessMessage"] = "You have successfully logged in!";

                    return Redirect(loginViewModel.ReturnUrl);
                }

                TempData["SuccessMessage"] = "You have successfully logged in!";

                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Failed to login. The input might be incorrect, please try again!";

            //Show errors
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            TempData["LogoutMessage"] = "You have succesfully logged out!";

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task GoogleLogin() 
        {

            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse"),
                    Parameters =
                    {
                        { "prompt", "login" } // Re-authenticate with Google
                    }
                });
        }
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (result?.Succeeded ?? false)
            {
                var userEmail = result.Principal?.FindFirst(ClaimTypes.Email)?.Value;

                if (!string.IsNullOrEmpty(userEmail))
                {
                    // Check if the user already exists in the database
                    var existingUser = await authDbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                    if (existingUser != null)
                    {
                        await signInManager.SignInAsync(existingUser, false);

                        TempData["SuccessMessage"] = "You have successfully logged in!";

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // User does not exist, create a new user
                        var identityUser = new IdentityUser
                        {
                            UserName = userEmail,
                            Email = userEmail
                        };

                        var generatedPass = PasswordGenerator();

                        var identityResult = await userManager.CreateAsync(identityUser, generatedPass);

                        if (identityResult.Succeeded)
                        {
                            var roleIdentityResult = await userManager.AddToRoleAsync(identityUser, "User");

                            // Sign in a new user
                            var signInResult = await signInManager.PasswordSignInAsync(identityUser,
                                                                                       generatedPass,
                                                                                       false,
                                                                                       false);

                            if (signInResult != null && signInResult.Succeeded)
                            {
                                TempData["SuccessMessage"] = "You have successfully logged in!";

                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                }
            }
            return View();
        }

        public async Task FacebookLogin()
        {

            await HttpContext.ChallengeAsync(FacebookDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("FacebookResponse"),
                    Parameters =
                    {
                        { "prompt", "login" } // Re-authenticate with Facebook
                    }                   
                });


        }

        public async Task<IActionResult> FacebookResponse()
        {
            var result = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);

            if (result?.Succeeded ?? false)
            {
                var userEmail = result.Principal?.FindFirst(ClaimTypes.Email)?.Value;

                if (!string.IsNullOrEmpty(userEmail))
                {
                    // Check if the user already exists in the database
                    var existingUser = await authDbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                    if (existingUser != null)
                    {
                        await signInManager.SignInAsync(existingUser, false);

                        TempData["SuccessMessage"] = "You have successfully logged in!";

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // User does not exist, create a new user
                        var identityUser = new IdentityUser
                        {
                            UserName = userEmail,
                            Email = userEmail
                        };

                        var generatedPass = PasswordGenerator();

                        var identityResult = await userManager.CreateAsync(identityUser, generatedPass);

                        if (identityResult.Succeeded)
                        {
                            var roleIdentityResult = await userManager.AddToRoleAsync(identityUser, "User");

                            // Sign in a new user
                            var signInResult = await signInManager.PasswordSignInAsync(identityUser,
                                                                                       generatedPass,
                                                                                       false,
                                                                                       false);

                            if (signInResult != null && signInResult.Succeeded)
                            {
                                TempData["SuccessMessage"] = "You have successfully logged in!";

                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                }
            }
            return View();
        }

        //public IActionResult GithubLogin(string returnUrl = null)
        // {
        //    // Construct the URL to the GitHub OAuth authorization endpoint
        //    var githubAuthEndpoint = "https://github.com/login/oauth/authorize";
        //    var clientId = "f69a4c5b6a40098fb65f"; // Replace with your GitHub OAuth app client ID
        //    var redirectUri = Url.Action("GithubResponse", "Account", new { ReturnUrl = returnUrl });
        //    var scope = "user:email"; // Add any additional scopes if required
        //    var state = ""; // You can generate a unique state parameter if needed

        //    // Construct the complete authorization URL
        //    var authorizationUrl = $"{githubAuthEndpoint}?client_id={clientId}&redirect_uri={redirectUri}&scope={scope}&state={state}";

        //    // Redirect the user to the GitHub OAuth authorization page
        //    return Redirect(authorizationUrl);

        //    //var properties = new AuthenticationProperties
        //    //{
        //    //    RedirectUri = Url.Action("GithubResponse")
        //    //};

        //    //return Challenge(properties, GitHubAuthenticationDefaults.AuthenticationScheme);

        //}

        //public async Task<IActionResult> GithubResponse(string returnUrl = null)
        //{
        //    var result = await HttpContext.AuthenticateAsync(GitHubAuthenticationDefaults.AuthenticationScheme);

        //    if (result?.Succeeded ?? false)
        //    {
        //        var userEmail = result.Principal?.FindFirst(ClaimTypes.Email)?.Value;

        //        if (!string.IsNullOrEmpty(userEmail))
        //        {
        //            // Check if the user already exists in the database
        //            var existingUser = await authDbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        //            if (existingUser != null)
        //            {
        //                await signInManager.SignInAsync(existingUser, false);

        //                TempData["SuccessMessage"] = "You have successfully logged in!";

        //                return RedirectToAction("Index", "Home");
        //            }
        //            else
        //            {
        //                // User does not exist, create a new user
        //                var identityUser = new IdentityUser
        //                {
        //                    UserName = userEmail,
        //                    Email = userEmail
        //                };

        //                var generatedPass = PasswordGenerator();

        //                var identityResult = await userManager.CreateAsync(identityUser, generatedPass);

        //                if (identityResult.Succeeded)
        //                {
        //                    var roleIdentityResult = await userManager.AddToRoleAsync(identityUser, "User");

        //                    // Sign in a new user
        //                    var signInResult = await signInManager.PasswordSignInAsync(identityUser,
        //                                                                               generatedPass,
        //                                                                               false,
        //                                                                               false);

        //                    if (signInResult != null && signInResult.Succeeded)
        //                    {
        //                        TempData["SuccessMessage"] = "You have successfully logged in!";

        //                        return RedirectToAction("Index", "Home");
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return View();
        //}

        [HttpGet]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());

            if (user != null)
            {
                var resetUserPassword = new ResetPasswordViewModel
                {
                    Username = user.UserName
                };
                return View(resetUserPassword);
            }

                return View(null);  
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordRequest)
        {
            ValidatePassword(resetPasswordRequest.newPassword);

            if (ModelState.IsValid)
            {
                // Check if the new password matches the confirm new password
                if (resetPasswordRequest.newPassword != resetPasswordRequest.confirmNewPassword)
                {
                    // Add model error if passwords don't match
                    ModelState.AddModelError("confirmNewPassword", "New password and confirm password do not match.");
                    return View(resetPasswordRequest);
                }

                // Find the user by their username
                var user = await userManager.FindByNameAsync(resetPasswordRequest.Username);

                // Check if the user exists
                if (user != null)
                {
                    // Check if the old password provided matches the one in the database
                    var passwordCheckResult = await userManager.CheckPasswordAsync(user, resetPasswordRequest.oldPassword);

                    if (passwordCheckResult)
                    {
                        // Change the password to the new one
                        var changePasswordResult = await userManager.ChangePasswordAsync(user, resetPasswordRequest.oldPassword, resetPasswordRequest.newPassword);

                        if (changePasswordResult.Succeeded)
                        {
                            // Password changed successfully
                            TempData["UpdatePassword"] = "You have succesfully reset your password!";
                            // Redirect to a success page or another appropriate action
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            // Handle the error if password change failed
                            ModelState.AddModelError(string.Empty, "Failed to change password.");
                        }
                    }
                    else
                    {
                        // Old password provided is incorrect
                        ModelState.AddModelError("oldPassword", "Old password is incorrect.");
                    }
                }
                else
                {
                    // User not found
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            // If ModelState is not valid or any other error occurs, return to the view with the model
            return View(resetPasswordRequest);
        }

        public IActionResult Cancel()
        {
            return RedirectToAction("Index", "Home");
        }
        private string PasswordGenerator()
        {
            var passChar = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()_+-=[]{}|;:,.<>?";
            var digits = "1234567890";

            StringBuilder password = new StringBuilder();
            Random random = new Random();

            // Adding at least one special character
            int specialCharIndex = random.Next(passChar.Length);
            password.Append(passChar[specialCharIndex]);

            // Adding at least one digit
            int digitIndex = random.Next(digits.Length);
            password.Append(digits[digitIndex]);

            for (int i = 0; i < 22; i++) // 24 - 2 (special char and digit)
            {
                int index = random.Next(passChar.Length);
                password.Append(passChar[index]);
            }
            return password.ToString();
        }

        private void ValidatePassword(string password)
        {
            if (password is not null)
            {
                if (!password.Any<char>(char.IsUpper))
                {
                    ModelState.AddModelError("Password", "Password has to contain at least one uppercase character");
                    ModelState.AddModelError("newPassword", "Password has to contain at least one uppercase character");
                }

                if (!password.Any<char>(char.IsNumber))
                {
                    ModelState.AddModelError("Password", "Password has to contain at least one number");
                    ModelState.AddModelError("newPassword", "Password has to contain at least one number");
                }

                if (!Regex.IsMatch(password, @"[!@#$%^&*()_+{}\[\]:;<>,.?\\|]"))
                {
                    ModelState.AddModelError("Password", "Password has to contain at least one special character");
                    ModelState.AddModelError("newPassword", "Password has to contain at least one special character");
                }
            }
        }



    }
}
