
using System.Security.Claims;
using AspNetCore.Identity.MongoDbCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDbGenericRepository;
using PetGameForum.Data;
using PetGameForum.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbClient = new MongoClient(connectionString);
var identityDbContext = new MongoDbContext(dbClient, "identity");

builder.Services.AddDefaultIdentity<User>(options => {
		options.SignIn.RequireConfirmedAccount = false;
		options.User.RequireUniqueEmail = true;
		options.Lockout.AllowedForNewUsers = false;
	})
	.AddRoles<Role>()
	.AddMongoDbStores<IMongoDbContext>(identityDbContext)
	.AddDefaultTokenProviders();

builder.Services.AddSingleton<MongoClient>(dbClient);
builder.Services.AddScoped<ForumService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddRazorPages();

var app = builder.Build(); 

{
	using var scope = app.Services.CreateScope();
	var roleManager = scope.ServiceProvider.GetService<RoleManager<Role>>();
	var role = new Role() {
		Name = "Admin",
	};
	role.Permissions = new []{Permission.SeeModeratorArea, Permission.EditRoles, Permission.AssignRoles};
	await roleManager.CreateAsync(role);
}

{
	using var scope = app.Services.CreateScope();
	var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
	var roleManager = scope.ServiceProvider.GetService<RoleManager<Role>>();
	var user = await userManager.FindByEmailAsync("cool@cool");
	user.AddRole((await roleManager.FindByNameAsync("Admin")).Id);
	await userManager.UpdateAsync(user);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseDeveloperExceptionPage();
}else{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
