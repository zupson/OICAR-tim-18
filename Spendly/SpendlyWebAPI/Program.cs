using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Services;
using SpendlyWebAPI.Utils;
using System.Text;

namespace SpendlyWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddDbContext<Models.SpendlyDbContext>(options => options.UseSqlServer("Name=ConnectionStrings:DatabaseConnStr"));

            var secureKey = builder.Configuration["JWT:SecureKey"];
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    var Key = Encoding.UTF8.GetBytes(secureKey);
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Key)
                    };
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<GroupService>();
            builder.Services.AddScoped<CostService>();
            builder.Services.AddScoped<CostTypeService>();
            builder.Services.AddScoped<RevenueService>();
            builder.Services.AddScoped<RevenueTypeService>();
            builder.Services.AddScoped<BudgetService>();
            builder.Services.AddScoped<InvitationService>();
            builder.Services.AddScoped<UserGroupService>();

            builder.Services.AddScoped<IAuthentication<ResponseUserDto>, UserService>();
            builder.Services.AddScoped<IInvitation<ResponseInvitationDto, CreateInvitationDto>, InvitationService>();
            builder.Services.AddScoped<IUserGroup<ResponseUserGroupDto>, UserGroupService>();
            builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
                    new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }},
                    Array.Empty<string>()
                }});
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            DbSeed.SeedAdmin(app.Services);

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}