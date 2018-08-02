using System.Collections.Generic;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections;
using JWT;
using JWT.Serializers;
using JWT.Algorithms;
using Microsoft.Extensions.Options;
using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using backend.Models;

namespace backend.Controllers
{
    public class JWTSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }

    //public class Credentials
    //{
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public string Email { get; set; }
    //    public string Password { get; set; }
    //}


    [Produces("application/json")]
    [Route("api/Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JWTSettings _options;

        public AccountController(
            UserManager<IdentityUser>_userManager, 
            SignInManager<IdentityUser>_signInManager,
            IOptions<JWTSettings> optionsAccessor)
        {
            this._userManager =_userManager;
            this._signInManager =_signInManager;
            this._options = optionsAccessor.Value;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] Credentials credentials)
        {
            
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = credentials.Email, Email = credentials.Email };
                var result = await _userManager.CreateAsync(user, credentials.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return new JsonResult(new Dictionary<string, object>
                    {
                        { "access_token", GetAccessToken(credentials.Email) },
                        { "id_token", GetIdToken(user) }
                    });
                }
                return Errors(result);

            }

            return Error("Unexpected error");
        }

        private string GetIdToken(IdentityUser user)
        {
            var payload = new Dictionary<string, object> {
                { "id", user.Id },
                { "sub", user.Email },
                { "email", user.Email },
                { "emailConfirmed", user.EmailConfirmed },
            };

            return GetToken(payload);
        }

        private string GetAccessToken(string Email)
        {
            var payload = new Dictionary<string, object> {
                { "sub", Email },
                { "email", Email }
            };

            return GetToken(payload);
        }

        private string GetToken(Dictionary<string, object> payload)
        {
            var secret = _options.SecretKey;

            payload.Add("iss", _options.Issuer);
            payload.Add("aud", _options.Audience);
            payload.Add("nbf", ConvertToUnixTimestamp(DateTime.Now));
            payload.Add("iat", ConvertToUnixTimestamp(DateTime.Now));
            payload.Add("exp", ConvertToUnixTimestamp(DateTime.Now.AddDays(7)));
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            return encoder.Encode(payload, secret);
        }

        private JsonResult Errors(IdentityResult result)
        {
            var items = result.Errors
                .Select(x => x.Description)
                .ToArray();
            return new JsonResult(items) { StatusCode = 400 };
        }

        private JsonResult Error(string message)
        {
            return new JsonResult(message) { StatusCode = 400 };
        }

        private static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        //==============================================================================
        //==============================================================================
        //==============================================================================
        //==============================================================================

        //[HttpPost("login")]
        //[AllowAnonymous]
        //public async Task<IActionResult> Login([FromBody]Credentials credentials)
        //{
        //    if (credentials == null) return BadRequest("Invalid Password or Email. Please try again.");

        //    var result = await _signInManager.PasswordSignInAsync(credentials.Email, credentials.Password, false, false);

        //    if (!result.Succeeded) return BadRequest("Invalid Password or Email. Please try again.");

        //    var user = await _userManager.FindByEmailAsync(credentials.Email);

        //    return Ok(CreateToken(user));
        //}

        //private bool ValidateCredentials(Credentials credentials)
        //{
        //    if (credentials == null) return false;

        //    // TODO

        //    return true;
        //}

        //private IRestResponse CreateToken(IdentityUser user)
        //{
        //    //var claims = new Claim[] {
        //    //    new Claim(JwtRegisteredClaimNames.Sub, user.Id)
        //    //};

        //    //// 30 minute expiration time
        //    //var expiration = DateTime.UtcNow.AddMinutes(30);

        //    //var default_Key = Encoding.UTF8.GetBytes("20m3r[0123jirm309r23jr923jnr0842n3fiom2piofn934fn9349rj945ngjnsdmnxm.gnkjfbdv;oenpg349rth438fenfkjer" +
        //    //                                         "wmefoefi3m2490fnmkwmfsldfERG#$Gm340fm3q4Fmrigno34FGWEG#$%923nr32DERHTHij9201230jaldjgo0982{SGJinzxcv");
        //    //var symmetricKey = "";

        //    //try
        //    //{   // Open the text file using a stream reader.
                
        //    //    using (StreamReader sr = new StreamReader("keys/key_private.asc"))
        //    //    {
        //    //        // Read the stream to a string, and write the string to the console.
        //    //        symmetricKey = sr.ReadToEnd();
        //    //    }
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    Trace.Write("The file could not be read:");
        //    //    Trace.Write(e.Message);
        //    //    return null;
        //    //}


        //    //// Fix this phrase to make more secure -- store in file

        //    //var signingKey = (symmetricKey != "") ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(symmetricKey)) : new SymmetricSecurityKey(default_Key);

        //    //var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        //    //var jwt = new JwtSecurityToken(signingCredentials: signingCredentials, claims: claims, expires: expiration);

        //    //return new JwtSecurityTokenHandler().WriteToken(jwt);

        //    var client = new RestClient("https://sebwb.au.auth0.com/oauth/token");
        //    var request = new RestRequest(Method.POST);
        //    request.AddHeader("content-type", "application/json");
        //    request.AddParameter("application/json", "{\"grant_type\":\"client_credentials\",\"client_id\": \"06k3572WpmVdvBnIiYqZGY1c6Kzy4uYn\",\"client_secret\": \"N1V9MruzlWFEUv093On0hYse7Z0IZoRhwAlYyMspT-9X-oZefYoEELWkFrUXFyc5\",\"audience\": \"https://eve.chatbot.ai\"}", ParameterType.RequestBody);
        //    IRestResponse response = client.Execute(request);
        //    return response;
        //}
    }
}
