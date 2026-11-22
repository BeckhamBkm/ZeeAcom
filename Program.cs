using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common;
using ZeeAcom.Common.Entities;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Text.Json.Serialization;
using ZeeAcom.Core.Properties.Commands;
using ZeeAcom.DataAccess;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Development + LocalDB: connect through the instance's named pipe (see LocalDbConnection for why).
var localDbInstance = builder.Environment.IsDevelopment() ? LocalDbConnection.GetInstanceName(connectionString) : null;
if (localDbInstance is not null)
{
    builder.Services.AddSingleton(sp => new LocalDbConnection(localDbInstance, sp.GetRequiredService<ILogger<LocalDbConnection>>()));
}

// Add services to the container.

// MVC: Razor views (Web/Controllers + Views/) and the JSON API (Api/Controllers) share this registration.
builder.Services.AddControllersWithViews(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .AddNewtonsoftJson(x => x.SerializerSettings.ContractResolver = new DefaultContractResolver())
            .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
            // Newtonsoft is the formatter in use, so enums ("Status": "Approved") are set to text here too.
            .AddNewtonsoftJson(x => x.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter()));

builder.Services.AddDbContext<DatabaseContext>((services, options) =>
{
    options.UseSqlServer(connectionString);
    if (localDbInstance is not null)
    {
        options.AddInterceptors(services.GetRequiredService<LocalDbConnection>());
    }
});
builder.Services.AddLazyCache();

// Property photos are stored on this server under wwwroot/uploads.
builder.Services.AddSingleton<ZeeAcom.Core.Pictures.IPictureStorage, LocalPictureStorage>();

// Enquiry emails to landlords (logged instead of sent until the "Smtp" settings are configured).
builder.Services.AddSingleton<ZeeAcom.Core.Notifications.IEmailSender, SmtpEmailSender>();

// The public enquiry form: at most 5 enquiries per visitor (IP address) every 10 minutes, to limit spam.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync(
            "You've sent several enquiries in a short time. Please wait a few minutes and try again, or contact the landlord on WhatsApp.",
            cancellationToken);
    };
    options.AddPolicy(RateLimits.Enquiries, context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
            }));
});

// Logins: Admin and Landlord accounts stored in the same database (AspNetUsers etc.)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

// Check the login's security stamp on every signed-in request (default is every 30 minutes), so a
// landlord the admin blocks is signed out straight away. Anonymous visitors aren't affected.
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;

    // API calls get 401/403 instead of a redirect to the login page.
    options.Events.OnRedirectToLogin = context => ApiAwareRedirect(context, StatusCodes.Status401Unauthorized);
    options.Events.OnRedirectToAccessDenied = context => ApiAwareRedirect(context, StatusCodes.Status403Forbidden);

    static Task ApiAwareRedirect(Microsoft.AspNetCore.Authentication.RedirectContext<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions> context, int statusCode)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = statusCode;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }

        return Task.CompletedTask;
    }
});


//AssemblyScanner.FindValidatorsInAssembly(typeof();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AddPropertyCommand>());
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ZeeAcom API",
        Version = "v1",
    });
    //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    //c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Create the Admin/Landlord roles and the admin account (from configuration) if they don't exist yet.
await IdentitySeeder.SeedAsync(app.Services, app.Configuration);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Attribute-routed API endpoints (/api/...)
app.MapControllers();

// Conventional MVC pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZeeAcom API V1"));

app.Run();
