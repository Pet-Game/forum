using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDbGenericRepository;
using PetGameForum.Data;
using PetGameForum.Services;
using PetGameForum.Util;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbClient = new MongoClient(connectionString);
var identityDbContext = new MongoDbContext(dbClient, "identity");

builder.Services.AddDefaultIdentity<User>(options => {
		options.SignIn.RequireConfirmedAccount = false;
		options.User.RequireUniqueEmail = true;
		options.Lockout.AllowedForNewUsers = true;
	})
	.AddRoles<Role>()
	.AddMongoDbStores<IMongoDbContext>(identityDbContext)
	.AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<MongoClient>(dbClient);
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<ForumService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

StaticStore.Services = app.Services;

{
	using var scope = app.Services.CreateScope();
	var roleManager = scope.ServiceProvider.GetService<RoleManager<Role>>();
	var role = new Role() {
		Name = "Admin",
	};
	role.Permissions = new List<Permission>{Permission.SeeModeratorArea, Permission.EditRoles, Permission.AssignRoles};
	await roleManager.CreateAsync(role);
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
app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
