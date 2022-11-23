using BlogMVC.Services;
using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using BugTrackerMVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ***** Use DataUtility to get Db Connection String
var connectionString = DataUtility.GetConnectionString(builder.Configuration);

// ***** change Sql server to Npgsql
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ***** Change default to BTUser
builder.Services.AddIdentity<BTUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ***** Custom Services
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IEmailSender, EmailService>();

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
