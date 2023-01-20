using BugTrackerMVC.Services;
using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using BugTrackerMVC.Extensions;

var builder = WebApplication.CreateBuilder(args);
// !!!!! commented out as not needed
// var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

// Add services to the container.

// ***** Use DataUtility to get Db Connection String
var connectionString = DataUtility.GetConnectionString(builder.Configuration);

// ***** change Sql server to Npgsql
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

// builder.Services.AddDefaultIdentity<BTUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ***** Change default to BTUser
builder.Services.AddIdentity<BTUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddClaimsPrincipalFactory<BTUserClaimsPrincipalFactory>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

// ***** Custom Services
builder.Services.AddScoped<IBTFileService, BTFileService>();
builder.Services.AddScoped<IEmailSender, EmailService>();
builder.Services.AddScoped<IBTRolesService, BTRolesService>();
builder.Services.AddScoped<IBTProjectService, BTProjectService>();
builder.Services.AddScoped<IBTTicketService, BTTicketService>();
builder.Services.AddScoped<IBTTicketHistoryService, BTTicketHistoryService>();
builder.Services.AddScoped<IBTCompanyService, BTCompanyService>();

// ***** MailSettings
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

// ***** Only one of the next two lines can be used
//          Use AddMvc() for better control
//builder.Services.AddControllersWithViews();
builder.Services.AddMvc(); 

var app = builder.Build();

// ***** Setup DataUtility Service
var scope = app.Services.CreateScope();
await DataUtility.ManageDataAsync(scope.ServiceProvider);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
