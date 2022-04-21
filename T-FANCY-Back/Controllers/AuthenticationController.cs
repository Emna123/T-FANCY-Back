using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using T_FANCY_Back.Models;
using T_FANCY_Back.Services;
using MimeKit;
namespace T_FANCY_Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        RoleManager<IdentityRole> roleManager;
        private readonly UserManager<User> UserManager;
        private readonly IJwtService _jwtService;
        private readonly TfancyContext _context;

        public AuthenticationController(IJwtService jwtService, UserManager<User> UserManager, UserManager<Manager> Umanager,

        RoleManager<IdentityRole> roleManager, TfancyContext context)
        {

            this.UserManager = UserManager;
            this.roleManager = roleManager;
            _context = context;
            _jwtService = jwtService;

        }

        //Login Manager
        [HttpPost]
        [Route("LoginManager")]
        public async Task<IActionResult> LoginManager([FromBody] AuthenticationRequest authRequest)
        {
            if (!ModelState.IsValid ) 
                return BadRequest(new AuthenticationResponse { IsSuccess = false, Reason = "Email and Password must be provided." });
            var user = UserManager.Users.FirstOrDefault(x => x.Email.Equals(authRequest.Email) && x.Password.Equals(authRequest.Password));
            if (user == null || !await UserManager.IsInRoleAsync(user, "Manager"))
            {
                return Unauthorized();
            }
            var authResponse = await _jwtService.GetTokensAsync(HttpContext.Connection.RemoteIpAddress.ToString(), user);
            return Ok(authResponse);           

        }

        //Login Client 
        [HttpPost]
        [Route("LoginClient")]
        public async Task<IActionResult> LoginClient([FromBody] AuthenticationRequest authRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthenticationResponse { IsSuccess = false, Reason = "Email and Password must be provided." });
            var user = UserManager.Users.FirstOrDefault(x => x.Email.Equals(authRequest.Email) && x.Password.Equals(authRequest.Password));
            if (user == null || !await UserManager.IsInRoleAsync(user, "Client"))
            {
                return Unauthorized();
            }
            var authResponse = await _jwtService.GetTokensAsync(HttpContext.Connection.RemoteIpAddress.ToString(), user);
            return Ok(authResponse);

        }

        //Logout Manger/Client
        [HttpPost]
        [Route("logout/{email}")]
        public async Task<IActionResult> logout([FromBody] UserRefreshToken reft, string email)
        {
            User user = await UserManager.FindByEmailAsync(email);
            if (user != null)
            {
                var savedRefreshToken = user.UserRefreshTokens.Where(x => x.RefreshToken == reft.RefreshToken &&
                x.Token == reft.Token).FirstOrDefault();

                if (savedRefreshToken != null)
                {
                    _context.Remove(savedRefreshToken);
                    _context.SaveChanges();
                    return Ok(new { msg = "success !" });
                }
                else
                {
                    return StatusCode(404);
                }
            }
            else
            {
                return StatusCode(403);
            }
        }

        //Create Refresh token
        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthenticationResponse { IsSuccess = false, Reason = "Tokens must be provided" });
            var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            var userRefreshToken =  _context.userRefreshToken.FirstOrDefault(
            x => x.IsInvalidated == false && x.Token == request.ExpiredToken
            && x.RefreshToken == request.RefreshToken
            && x.IpAddress == ipAddress);        
            AuthenticationResponse response = ValidateDetails(request.ExpiredToken, userRefreshToken);
            if (!response.IsSuccess)
                return BadRequest(response);
            userRefreshToken.IsInvalidated = true;
            _context.userRefreshToken.Update(userRefreshToken);
            Console.WriteLine(userRefreshToken);
            await _context.SaveChangesAsync();
            var roles = await UserManager.GetRolesAsync(userRefreshToken.user);
            var authResponse =await _jwtService.GetTokensAsync(ipAddress, userRefreshToken.user);
            return Ok(authResponse);

        }

        //Validate Token
        private AuthenticationResponse ValidateDetails(string expiredToken, UserRefreshToken userRefreshToken)
        {
            if (userRefreshToken == null)
                return new AuthenticationResponse{IsSuccess = false, Reason = "Invalid Token Details." };
            var token = GetJwtToken(expiredToken);
            if (token.ValidTo > DateTime.UtcNow)
                return new AuthenticationResponse{IsSuccess =true, Reason= "Token not expired." };
            if (!userRefreshToken.ISActive)
                return new AuthenticationResponse{IsSuccess= false, Reason= "Refresh Token Expired" };
            return new AuthenticationResponse { IsSuccess = true };
}
        //return token
        private JwtSecurityToken GetJwtToken(string expiredToken)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ReadJwtToken(expiredToken);
        }

        //Register Client
        [HttpPost]
        [Route("RegisterClient")]
        public async Task<IActionResult> Register([FromBody] Client model)
        {
            var userExist = await UserManager.FindByEmailAsync(model.Email);
            var Username = await UserManager.FindByNameAsync(model.Email);

            Console.Write("userExist", userExist);

            if ((userExist != null || Username!=null ) )
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "This email address is already in use!");
            }


               Client Client = new Client
            {      
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                Password = model.Password,
                NormalizedEmail = model.Email.ToUpper(),
                FirstName = model.FirstName,
                LastName=model.LastName,
                Sex=model.Sex,
                Birth=model.Birth,
                UserName=model.Email,
                Inscri_news=model.Inscri_news,
                UserRefreshTokens=null

            };
            var result = await UserManager.CreateAsync(Client, model.Password);
            bool x = await roleManager.RoleExistsAsync("Client");

            if (!x)
            {
                var role = new IdentityRole();
                role.Name = "Client";
                await roleManager.CreateAsync(role);
            }

            var result1 = await UserManager.AddToRoleAsync(Client, "Client");
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "something is wrong please try again !");

            }
            return  StatusCode(StatusCodes.Status200OK, "User succefully added !");

        }

        //Register Manager
        [HttpPost]
        [Route("RegisterManager")]
        public async Task<IActionResult> RegisterManager([FromBody] Manager model)
        {
            var userExist = await UserManager.FindByEmailAsync(model.Email);
            var Username = await UserManager.FindByNameAsync(model.Email);
            Console.Write("userExist", userExist);
            if (userExist != null || Username != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "This email address is already in use!");
            }
                Manager manager = new Manager
            {
                Email = model.Email,
                NormalizedEmail= model.Email.ToUpper(),
                    UserName = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
            };
            var result = await UserManager.CreateAsync(manager, model.Password);
            bool x = await roleManager.RoleExistsAsync("Manager");

            if (!x)
            {
                var role = new IdentityRole();
                role.Name = "Manager";
                await roleManager.CreateAsync(role);
            }

            var result1 = await UserManager.AddToRoleAsync(manager, "Manager");
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "something is wrong please try again !");

            }
            return StatusCode(StatusCodes.Status200OK, "User succefully added !");

        }


        //reset password user
        [HttpPost]
        [Route("RequestPassword")]
        public async Task<IActionResult> RequestPassword([FromBody] ResetPassword res)
        {
            var email = res.email;
            Console.WriteLine("this a message from reset password : " + email);
            Manager manager =await  _context.manager.FirstOrDefaultAsync(x => x.Email == email);
            if (manager != null)
            {
                var token = await UserManager.GeneratePasswordResetTokenAsync(manager);
                token = HttpUtility.UrlEncode(token);
                Console.WriteLine("this a message from reset password : " + token);
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("  ", "t.fancy.manager@gmail.com"));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = " Rénitialiser votre mot de passe ";
                var url = "http://localhost:4200/auth/reset-password?user_id=" + manager.Email + "&token=" + token;
                message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = string.Format("<br/><br/><img src='{0}' style='height: 50px;width: 150px;margin-left: 110px;'/><hr style='color:#9B59B6;width: 600px;margin-left: 100px;'>", "https://i.ibb.co/7Wp9j38/argon-white.png") + string.Format("<p style='margin-left: 110px;font-size: 15px;'>Bonjour," + manager.FirstName.ToUpper() + " " + manager.LastName +"{0}</p>", "<p style='margin-left: 110px;font-size: 15px;'>Pour changer votre mot de passe, veuillez cliquer sur le bouton ci-dessous :<br/><br/></p>") + string.Format("<a href={1}><button  style='background-color:#681F19 ;color:#FEFEFE ;margin-left: 110px;border: 1px solid #681F19 ;padding:20px;font-size: 15px;border-radius: 10px;'>Changer votre mot de passe</button></a><p style='margin-left: 110px;font-size: 15px;'></br><br/>Merci pour votre confiance,</p><p style='margin-left: 110px;font-size: 15px;'> L'équipe T-Fancy</p></ p>", 123,  url)
                };
                using (var mngr = new MailKit.Net.Smtp.SmtpClient())
                {
                    mngr.CheckCertificateRevocation = false;
                    mngr.Connect("smtp.gmail.com", 587, false);
                    mngr.Authenticate("t.fancy.manager@gmail.com", "t-fany@123");
                    mngr.Send(message);
                    mngr.Disconnect(true);
                }
                return Ok(new  { message= "Email successfully sent" });
            }
            else
            {
                return NotFound();
            }
        }

        //Reset password 
        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> changePassword([FromBody] ResetPassword res)
        {
            try
            {
                User Manager = await UserManager.FindByEmailAsync(res.email);
                res.token=res.token.Replace("+", "%2b");
                res.token=res.token.Replace("/", "%2f");
                if (Manager != null && res.newpassword != null && res.token!=null)
                {
                    var email = Manager.Email;

                    var hashPass = UserManager.PasswordHasher.HashPassword(Manager, res.newpassword);
                    Manager.Password = res.newpassword;
                    Manager.PasswordHash = hashPass;
                    var result= await UserManager.ResetPasswordAsync(Manager, HttpUtility.UrlDecode(res.token),res.newpassword);
                  
                    if (result.Succeeded)
                    {
                        Manager.NormalizedEmail = email.ToUpper();

                        _context.Entry(Manager).State = EntityState.Modified;
                        _context.SaveChanges();

                        return Ok(new
                        {
                            msg = "Password changed succefully !"
                        }); 
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK,result.Errors);

                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch
            {
                return BadRequest();
            }
        }
      


    }
}
