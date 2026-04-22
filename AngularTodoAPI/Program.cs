using AngularTodoAPI.Data;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;


namespace AngularTodoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {

            /**************************************************************
            *** Builder.Services: this is where we register services that our application will use, such as controllers, database contexts, authentication services, etc. These services are then available for dependency injection throughout the application.
            ***************************************************************/
            var builder = WebApplication.CreateBuilder(args);

            // Register services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
            });


            // Register the DbContext with the connection string from appsettings.json
            builder.Services.AddDbContext<TodoContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register CORS policy to allow requests from Angular development server
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularDev",
                     policy => policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                                     .AllowAnyHeader()
                                     .AllowAnyMethod());
            });

            // Register authentication services and configure JWT authentication
            var jwtSettings = builder.Configuration.GetSection("Jwt"); // get JWT settings from appsettings.json
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]); // get the secret key for signing JWTs

            builder.Services.AddAuthentication(options =>   
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // set default authentication scheme to JWT
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // this scheme will be used when the user tries to access a protected resource without being authenticated
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, // validate the issuer of the token
                    ValidateAudience = true, // validate the audience of the token
                    ValidateLifetime = true, // validate the expiration time of the token
                    ValidateIssuerSigningKey = true, // validate the signing key of the token
                    ValidIssuer = jwtSettings["Issuer"], // set the valid issuer from appsettings.json
                    ValidAudience = jwtSettings["Audience"], // set the valid audience from appsettings.json
                    IssuerSigningKey = new SymmetricSecurityKey(key) // set the signing key for validating JWTs
                };
            });

            builder.Services.AddAuthorization(); // this allows us to use the [Authorize] attribute in our controllers to protect certain endpoints





            /**************************************************************
            *** app.Use...: defines the middleware pipeline for handling HTTP requests. The order of these calls is important, as it determines how requests are processed and which middleware gets executed first.
            ***************************************************************/
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAngularDev");

            app.UseAuthentication(); // this enables authentication middleware to validate JWTs on incoming requests
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
