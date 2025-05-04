using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Business.Services;
using TaskAspNet.Data.Context;
using TaskAspNet.Data.Entities;
using TaskAspNet.Data.Interfaces;
using TaskAspNet.Data.EntityIdentity;
using TaskAspNet.Data.Repositories;
using TaskAspNet.Services.Interfaces;
using TaskAspNet.Services;
using TaskAspNet.Web.Hubs;
using TaskAspNet.Web.Interfaces;
using TaskAspNet.Web.Services;
using System.Text.Json.Serialization;
using TaskAspNet.Web.Infrastructure.Factories;
using Serilog;
using TaskAspNet.Web.Infrastructure;
using TaskAspNet.Web.Factories;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/webapp-.txt",
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 20 * 1024 * 1024,
        rollOnFileSizeLimit: true,
        retainedFileCountLimit: 30,
        buffered: true)
    .CreateLogger();


builder.Host.UseSerilog();

var mvc = builder.Services
    .AddControllersWithViews(opts =>
    {
        
        opts.Filters.Add<ValidateModel>();
        opts.Filters.Add<ActionLogging>();        
        opts.Filters.Add<RequireMemberProfile>();
    });

mvc.AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddSingleton<ActionLogging>();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; 
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/LogIn";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

// Remove before start up otherwise you get ArgumentNullException: Value cannot be null. (Parameter 'ClientId')
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//})
//    .AddCookie()
//    .AddGoogle(googleOptions =>
//    {
//        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
//        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
//    });

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationHubService, NotificationHubService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IProjectStatusRepository, ProjectStatusRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationTypeRepository, NotificationTypeRepository>();
builder.Services.AddScoped<IBaseRepository<Consent>, BaseRepository<Consent>>();
builder.Services.AddScoped<IConsentRepository, ConsentRepository>();
builder.Services.AddScoped<IConsentService, ConsentService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IFileService, FileService>();
//builder.Services.AddScoped<ILoggerService, LoggerService>();
builder.Services.AddScoped<IGoogleAuthHandler, GoogleAuthHandler>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<IMemberIndexVmFactory, MemberIndexVmFactory>();
builder.Services.AddScoped<IProjectIndexVmFactory, ProjectIndexVmFactory>();

// the image helper shown earlier
builder.Services.AddSingleton<IImageService, ImageService>();


var app = builder.Build();

app.UseHsts();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=LogIn}/{id?}");

app.MapHub<NotificationHub>("/notificationHub", options =>
{
    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
    options.CloseOnAuthenticationExpiration = true;
});

await app.Services.SeedSuperAdminAsync();


app.Run();

