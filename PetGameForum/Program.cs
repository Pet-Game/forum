
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDbGenericRepository;
using PetGameForum.Data;

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

builder.Services.AddRazorPages();

var app = builder.Build();

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