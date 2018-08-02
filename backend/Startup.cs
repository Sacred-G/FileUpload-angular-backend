using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Diagnostics;
using System.IO;
using backend.Controllers;

namespace backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JWTSettings>(Configuration.GetSection("JWTSettings"));

            services.AddCors(options => options.AddPolicy("Cors", builder => {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddDbContext<UserDbContext>(opt => opt.UseInMemoryDatabase("user"));

            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<UserDbContext>();

            // Configuring Authentication services

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.Authority = "https://sebwb.au.auth0.com/";
                options.Audience = "https://eve.chabot.ai";
            });


            /*
            // Using Custom signing key

            var default_Key = Encoding.UTF8.GetBytes("20m3r[0123jirm309r23jr923jnr0842n3fiom2piofn934fn9349rj945ngjnsdmnxm.gnkjfbdv;oenpg349rth438fenfkjer" +
                                                     "wmefoefi3m2490fnmkwmfsldfERG#$Gm340fm3q4Fmrigno34FGWEG#$%923nr32DERHTHij9201230jaldjgo0982{SGJinzxcv");
            var symmetricKey = "";

            try
            {   // Open the text file using a stream reader.

                using (StreamReader sr = new StreamReader("keys/key_private.asc"))
                {
                    // Read the stream to a string, and write the string to the console.
                    symmetricKey = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Trace.Write("The file could not be read:");
                Trace.Write(e.Message);
            }

            var signingKey = (symmetricKey != "") ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(symmetricKey)) : new SymmetricSecurityKey(default_Key);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;

                // Change settings for production
                cfg.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = signingKey,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });
            */

            // Angular's default header name for sending the XSRF token.
            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            services.AddMvc();

            // For allowing file uploads greater than 134,217,728 Bytes
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });

            //services.AddHsts(options =>
            //{
            //    options.Preload = true;
            //    options.IncludeSubDomains = true;
            //    options.MaxAge = TimeSpan.FromDays(60);
            //    options.ExcludedHosts.Add("example.com");
            //    options.ExcludedHosts.Add("www.example.com");
            //});

            //services.AddHttpsRedirection(options =>
            //{
            //    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            //    options.HttpsPort = 5001;
            //});
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Error");
                //app.UseHsts();
            }

            // Enable authentication middleware
            app.UseAuthentication();

            //app.UseHttpsRedirection();

            app.UseCors("Cors");
            //app.UseStaticFiles();
            //app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}
