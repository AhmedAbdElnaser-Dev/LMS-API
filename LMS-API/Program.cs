using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Services;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 1. Configure Authentication first
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddBearerToken(IdentityConstants.BearerScheme) 
.AddIdentityCookies();  

// 2. Configure Identity Core with RoleManager Fix
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
})
.AddRoles<IdentityRole>() 
.AddEntityFrameworkStores<DBContext>()
.AddDefaultTokenProviders()
.AddSignInManager()
.AddApiEndpoints();  

// Register RoleManager explicitly
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// 3. Configure Authorization
builder.Services.AddAuthorization();

// 4. Configure Application Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.Cookie.MaxAge = TimeSpan.FromDays(5);
    options.SlidingExpiration = true;
    options.Cookie.IsEssential = true;
});

// 5. Controllers and services
builder.Services.AddControllers();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// 6. Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "LMS_API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// 7. CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 8. Email service
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, NoOpEmailSender>();

// 9. Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// 10. Middleware pipeline
app.UseAuthentication();  // Must come first
app.UseAuthorization();

// 11. Map Identity API endpoints
app.MapIdentityApi<ApplicationUser>();

// 12. Development configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 13. Apply CORS policy
app.UseCors("AllowSpecificOrigins");

// 14. Seed roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleSeeder.SeedRoles(services);
    await CategorySeeder.SeedCategories(services);
}

// 15. Other Middleware
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
