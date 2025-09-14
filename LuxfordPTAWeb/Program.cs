using LuxfordPTAWeb.Components;
using LuxfordPTAWeb.Components.Account;
using LuxfordPTAWeb.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LuxfordPTAWeb;

public class Program
{
    public static async Task Main(string[] args)  // Make Main async
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization();

        // ADD THIS: Add controllers support
        builder.Services.AddControllers();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()  // Add this line for role support
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

			builder.Services.AddHttpClient("Default", client =>
			{
				client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001/");
			});

			var app = builder.Build();

			// FIXED: Proper async seeding
			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				try
				{
					await SeedRolesAndAdminUser(services);
				}
				catch (Exception ex)
				{
					var logger = services.GetRequiredService<ILogger<Program>>();
					logger.LogError(ex, "An error occurred while seeding roles and admin user.");
				}
			}


			// Place the migration code here
			if (!app.Environment.IsDevelopment())
			{
				using var scope = app.Services.CreateScope();
				var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
				var markerFile = Path.Combine(env.ContentRootPath, "run-migration.txt");

				if (File.Exists(markerFile))
				{
					var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
					using var writer = File.AppendText(markerFile);
					writer.WriteLine("======");
					writer.WriteLine($"Migration run at {DateTime.Now:u}");
					try
					{
						db.Database.Migrate();
						writer.WriteLine("Migration completed successfully.");
					}
					catch (Exception ex)
					{
						writer.WriteLine($"Migration failed: {ex.Message}");
					}
				}
			}

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        // ADD THIS: Map controller endpoints
        app.MapControllers();

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        await app.RunAsync();  // Use RunAsync since Main is now async
    }


	// Add this method at the end of the Program class:
	static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
	{
		var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
		var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

		// Create roles
		string[] roleNames = { "Admin", "BoardMember", "Volunteer" };
		foreach (var roleName in roleNames)
		{
			if (!await roleManager.RoleExistsAsync(roleName))
			{
				await roleManager.CreateAsync(new IdentityRole(roleName));
			}
		}

		// Create admin user
		var adminEmail = "jonathan@delfraisse.com";
		var adminUser = await userManager.FindByEmailAsync(adminEmail);
		if (adminUser == null)
		{
			adminUser = new ApplicationUser
			{
				UserName = adminEmail,
				Email = adminEmail,
				EmailConfirmed = true,
				FirstName = "Jonathan",
				LastName = "Delfraisse"
			};
			await userManager.CreateAsync(adminUser, "Admin123!"); // Use a strong password
			await userManager.AddToRoleAsync(adminUser, "Admin");
		}
	}
}
