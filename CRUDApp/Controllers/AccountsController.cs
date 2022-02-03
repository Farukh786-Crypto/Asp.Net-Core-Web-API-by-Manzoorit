using CRUDApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CRUDApp.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        // UserManager is for create user   
        UserManager<IdentityUser> userManager;
        // SignInManager is used for singint and signout (login and logout) 
        SignInManager<IdentityUser> signInManager;
        public AccountsController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {

                IdentityUser user = new IdentityUser()
                {
                    UserName = model.Username,
                    Email = model.Email
                };

                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // bydefault all register user are user so we assign user role
                    var resultRole = await userManager.AddToRoleAsync(user,"U");
                    if(resultRole.Succeeded)
                    {
                        return Ok(new { Name = user.UserName, Email = user.Email });
                    }
                    else
                    {
                        // if user is created without role then delete user
                        await userManager.DeleteAsync(user);
                        return BadRequest(resultRole.Errors);
                    }
                    
                }
                else
                {

                }
                return BadRequest(result.Errors);
            }
            else
             {
                return BadRequest(ModelState);
            }
        }
        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            if(ModelState.IsValid)
            {
                var Result = await signInManager.PasswordSignInAsync(model.UserName,model.Password,model.RememberMe,true);

                if(Result.Succeeded)
                {
                    var user = await userManager.FindByNameAsync(model.UserName);
                    var roles = await userManager.GetRolesAsync(user);

                    // step - 1: Create IdentityClaims

                    IdentityOptions identityOptions = new IdentityOptions();

                    var claims = new Claim[]
                    {
                        new Claim("Lid","123456789"),
                        new Claim(identityOptions.ClaimsIdentity.UserIdClaimType,user.Id),
                        new Claim(identityOptions.ClaimsIdentity.UserNameClaimType,user.UserName),
                        new Claim(identityOptions.ClaimsIdentity.RoleClaimType,roles[0])
                    };

                    // step - 2:  signinkey contain ueserkey and we apply algorith
                    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this-is-my-jwt-secret-key"));

                    // step - 3: Create signingCredentials from signingkey with HMAC Algorithm
                    var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

                    // step - 4: Create JWT with SigningCredentials , IdentityClaims & expire duration.
                    var jwt = new JwtSecurityToken(claims:claims, signingCredentials: signingCredentials, expires:DateTime.Now.AddMinutes(40));

                    // step - 5: Finally write the tokenas response with ok
                    return Ok(new {token=new JwtSecurityTokenHandler().WriteToken(jwt),
                                    userName=user.UserName,
                                    role= roles[0]
                    });
                }
                else
                {
                    return BadRequest(new { Msg="UserName and Password is invalid" });
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
         
        }
        [HttpPost("SignOut")]
        public async Task<IActionResult> SignOut()
        {
            await signInManager.SignOutAsync();
            return Ok(); 
        }

    }
}
