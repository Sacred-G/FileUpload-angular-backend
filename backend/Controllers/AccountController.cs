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
using System.Security.Cryptography.X509Certificates;
using System.IO;
using RestSharp;
using Newtonsoft.Json;

namespace backend.Controllers
{

    public class Resp
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
    };

    public class JWTSettings
    {
        public string Cert { get; set; }
        public string SecretKey { get; set; }
        public string ClientId { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }

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

            return Error("Invalid registration details. Please try again.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> SignIn([FromBody] Credentials Credentials)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Credentials.Email, Credentials.Password, false, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(Credentials.Email);
                    return new JsonResult(new Dictionary<string, object>
                    {
                        { "access_token", GetAccessToken(Credentials.Email) },
                        { "id_token", GetIdToken(user) }
                    });
                }

                return new JsonResult("Unable to sign in") { StatusCode = 401 };
            }

            return Error("Invalid email or password. Please try again.");
        }

        private string GetIdToken(IdentityUser user)
        {
            var payload = new Dictionary<string, object> {
                { "id", user.Id },
                { "sub", _options.ClientId + "@clients" },
                { "email", user.Email },
                { "emailConfirmed", user.EmailConfirmed },
            };

            return GetToken(payload);
        }

        private string GetAccessToken(string Email)
        {
            //var payload = new Dictionary<string, object> {
            //    { "sub", _options.ClientId + "@clients" },
            //    { "email", Email }
            //};

            //return GetToken(payload);



            var client = new RestClient(String.Format("{}/oauth/token", _options.Issuer));
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            //request.AddParameter("application/json", "{\"client_id\":\"" + _options.ClientId + "\",\"client_secret\":\"" + _options.SecretKey + "\",\"audience\":\"" + _options.Audience + "\",\"grant_type\":\"client_credentials\"}", ParameterType.RequestBody);
            request.AddParameter("application/json", "{\"client_id\":\"06k3572WpmVdvBnIiYqZGY1c6Kzy4uYn\",\"client_secret\":\"N1V9MruzlWFEUv093On0hYse7Z0IZoRhwAlYyMspT-9X-oZefYoEELWkFrUXFyc5\",\"audience\":\"https://eve.chabot.ai\",\"grant_type\":\"client_credentials\"}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Resp resp = JsonConvert.DeserializeObject<Resp>(response.Content);

            return resp.access_token;
        }

        //Reads a file.
        internal static byte[] ReadFile(string fileName)
        {
            FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            int size = (int)f.Length;
            byte[] data = new byte[size];
            size = f.Read(data, 0, size);
            f.Close();
            return data;
        }

        private string GetToken(Dictionary<string, object> payload)
        {
            var secret = _options.SecretKey;

            //Create X509Certificate2 object from .cer file.
            byte[] rawData = ReadFile("./keys/" + _options.Cert);

            // Load the certificate into an X509Certificate object.
            var x509 = new X509Certificate2(rawData);

            payload.Add("iss", _options.Issuer);
            payload.Add("aud", _options.Audience);
            payload.Add("nbf", ConvertToUnixTimestamp(DateTime.Now));
            payload.Add("iat", ConvertToUnixTimestamp(DateTime.Now));
            payload.Add("exp", ConvertToUnixTimestamp(DateTime.Now.AddDays(7)));
            payload.Add("azp", _options.ClientId);
            payload.Add("gty", "client-credentials");
            //IJwtAlgorithm algorithm = new RS256Algorithm(x509);
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
    }
}
