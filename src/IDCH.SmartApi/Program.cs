using IDCH.SmartApi.Helpers;
using IDCH.Tools;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models;
using Models.Yolo;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IDCH.SmartApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddTransient<AzureBlobHelper>();
            builder.Services.AddTransient<YoloModel>();


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin().AllowAnyHeader().WithMethods("GET, PATCH, DELETE, PUT, POST, OPTIONS"));
            });
            var configBuilder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false);
            IConfiguration Configuration = configBuilder.Build();


            AppConstants.UploadUrlPrefix = Configuration["UploadUrlPrefix"];

            builder.Services.AddControllers();
            var securityScheme = new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JSON Web Token based security",
            };

            var securityReq = new OpenApiSecurityRequirement()
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }
};

            var contact = new OpenApiContact()
            {
                Name = "Gravicode Team",
                Email = "company@gravicode.com",
                Url = new Uri("https://simadafarmalkes.kemkes.go.id")
            };

            var license = new OpenApiLicense()
            {
                Name = "Smart API License",
                Url = new Uri("https://gravicode.com/smart-api-license.html")
            };

            var info = new OpenApiInfo()
            {
                Version = "v1",
                Title = "Smart API v1.0",
                Description = "API for accessing Smart Api",
                TermsOfService = new Uri("https://gravicode.com/terms.html"),
                Contact = contact,
                License = license
            };

            // Add JWT configuration (support multi auth scheme, jwt and cookie)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey
                            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                    CookieAuthenticationDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme);
                defaultAuthorizationPolicyBuilder =
                    defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", info);
                o.AddSecurityDefinition("Bearer", securityScheme);
                o.AddSecurityRequirement(securityReq);
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart API V1");
                });
            }

            //app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true) // allow any origin  
            .AllowCredentials());               // allow credentials 

            app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

            var summaries = new[]
            {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateTime.Now.AddDays(index),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast");

            app.MapPost("/detectobject",
       async Task<IResult> (HttpRequest request, YoloModel yolo) =>
       {
           if (!request.HasFormContentType)
               return Results.BadRequest();

           var form = await request.ReadFormAsync();
           var formFile = form.Files["file"];

           if (formFile is null || formFile.Length == 0)
               return Results.BadRequest();

           await using var stream = formFile.OpenReadStream();
           OutputCls res = new OutputCls() { IsSucceed = false };
           using (BinaryReader reader = new BinaryReader(stream))
           {
               var bytes = reader.ReadBytes((int)stream.Length);
               res = await yolo.Predict(bytes);
           }
           //var reader = new StreamReader(stream);
           //var text = await reader.ReadToEndAsync();
           var uri = new Uri(request.GetDisplayUrl());
           var baseUrl = uri.GetLeftPart(System.UriPartial.Authority);
           if (res.IsSucceed)
           {
               for (var i = 0; i < res.FileUrls.Count; i++)
               {
                   res.FileUrls[i] = $"{baseUrl}{AppConstants.UploadUrlPrefix}{res.FileUrls[i]}";
               }
           }
           return Results.Ok(res);
       })
            .WithName("DetectObject");
            app.MapPost("/security/getToken", [AllowAnonymous] (UserDto user) =>
            {

                if (user.UserName == "admin" && user.Password == "123qweasd")
                {
                    var issuer = builder.Configuration["Jwt:Issuer"];
                    var audience = builder.Configuration["Jwt:Audience"];
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    // Now its ime to define the jwt token which will be responsible of creating our tokens
                    var jwtTokenHandler = new JwtSecurityTokenHandler();

                    // We get our secret from the appsettings
                    var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

                    // we define our token descriptor
                    // We need to utilise claims which are properties in our token which gives information about the token
                    // which belong to the specific user who it belongs to
                    // so it could contain their id, name, email the good part is that these information
                    // are generated by our server and identity framework which is valid and trusted
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[]
                        {
                new Claim("Id", "1"),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                // the JTI is used for our refresh token which we will be convering in the next video
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
                        // the life span of the token needs to be shorter and utilise refresh token to keep the user signedin
                        // but since this is a demo app we can extend it to fit our current need
                        Expires = DateTime.UtcNow.AddHours(6),
                        Audience = audience,
                        Issuer = issuer,
                        // here we are adding the encryption alogorithim information which will be used to decrypt our token
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
                    };

                    var token = jwtTokenHandler.CreateToken(tokenDescriptor);

                    var jwtToken = jwtTokenHandler.WriteToken(token);

                    return Results.Ok(jwtToken);
                }
                else
                {
                    return Results.Unauthorized();
                }
            });
            app.Run();
        }
        record UserDto(string UserName, string Password);

    }
}