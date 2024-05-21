using Bloggie.Web.Data;
using Bloggie.Web.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Net.Http.Headers;
using AspNet.Security.OAuth.GitHub;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
        options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
    });

builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.ClientId = "389846274018884";
    options.ClientSecret = "a2288096997599d9c9ed349ac970e568";
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GitHubAuthenticationDefaults.AuthenticationScheme;
}).AddGitHub(options =>
{
    options.ClientId = "f69a4c5b6a40098fb65f";
    options.ClientSecret = "598eca764494d99c65670fa60c2515457f73092c";
    options.Scope.Add("user:email");
    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    //options.CallbackPath = new PathString("/signin-github");
});

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = "GitHub"; // custom scheme
//})
//  .AddOAuth("GitHub", options =>
//    {
//        options.ClientId = "f69a4c5b6a40098fb65f";
//        options.ClientSecret = "598eca764494d99c65670fa60c2515457f73092c";
//        options.CallbackPath = new PathString("/signin-github");

//        options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
//        options.TokenEndpoint = "https://github.com/login/oauth/access_token";
//        options.UserInformationEndpoint = "https://api.github.com/user";

//        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
//        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
//        options.ClaimActions.MapJsonKey("urn:github:name", "name");
//        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
//    });

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = "GitHub"; // Custom scheme for GitHub
//})
//.AddCookie()
//.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
//{
//    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
//    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
//})
//.AddFacebook(options =>
//{
//    options.ClientId = "389846274018884";
//    options.ClientSecret = "a2288096997599d9c9ed349ac970e568";
//})
//.AddOAuth("GitHub", options =>
//{
//    options.ClientId = "f69a4c5b6a40098fb65f";
//    options.ClientSecret = "598eca764494d99c65670fa60c2515457f73092c";
//    options.CallbackPath = new PathString("/signin-github");

//    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
//    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
//    options.UserInformationEndpoint = "https://api.github.com/user";

//    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
//    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
//    options.ClaimActions.MapJsonKey("urn:github:name", "name");
//    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
//});



// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BloggieDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("BloggieDbConnectionString")));

builder.Services.AddDbContext<AuthDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("BloggieAuthDbConnectionString")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    //Default settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredUniqueChars = 1;
});

builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IImageRepository, CloudinaryImageRepository > ();
builder.Services.AddScoped<IBlogPostLikeRepository, BlogPostLikeRepository>();
builder.Services.AddScoped<IBlogPostCommentRepository, BlogPostCommentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();
//app.Urls.Add("https://bloggiewebdp.azurewebsites.net/");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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

app.Run();
