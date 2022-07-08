using Identity_API_Demo.Entity;
using Identity_API_Demo.Infrastructure;
using Identity_API_Demo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


    #region Service

    // Add services to the container.
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();

    #region Swagger

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => {

        // Config UI for input JWToken.
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTToken_Auth_API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
        // Use JWT has input to Authenticate.
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });
    #endregion

    // Add Connection string
    builder.Services.AddDbContext<IdentityDemoDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnectionString"));
    });

    // Add Authenticate Service (Dependency Injection)
    builder.Services.AddScoped<IAuthenService, AuthenService>();

    // Add service for Identity
    builder.Services.AddIdentity<Customer, IdentityRole>()
        .AddRoles<IdentityRole>()                          // Add role service.
        .AddEntityFrameworkStores<IdentityDemoDbContext>();

    // Config password ruler in IdentityUser
    builder.Services.Configure<IdentityOptions>(options =>
    {
        // Password settings.
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 5;
        options.Password.RequiredUniqueChars = 1;
    });

    // Add service of JWT
    builder.Services.AddAuthentication(x =>
    {
	    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(o =>
    {
	    // Read key from appsetting to hash
	    var Key = Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]);
	    o.SaveToken =true;
	    o.TokenValidationParameters = new TokenValidationParameters
	    {
		    ValidateIssuer = false,
		    ValidateAudience = false,
		    ValidateLifetime = true,
		    ValidateIssuerSigningKey = true,
		    ValidIssuer = builder.Configuration["JWT:Issuer"],
		    ValidAudience = builder.Configuration["JWT:Audience"],
		    IssuerSigningKey = new SymmetricSecurityKey(Key)
	    };
    });


    #endregion

    #region Configure

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Add service for Authenticate.
    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();

    #endregion