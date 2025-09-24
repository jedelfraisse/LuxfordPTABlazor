using LuxfordPTAWeb.Client.Code;
using LuxfordPTAWeb.Client.Services;
using LuxfordPTAWeb.Components;
using LuxfordPTAWeb.Components.Account;
using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.Configuration;
using LuxfordPTAWeb.Shared.Models;
using LuxfordPTAWeb.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace LuxfordPTAWeb;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents()
			.AddInteractiveWebAssemblyComponents()
			.AddAuthenticationStateSerialization();

		// Add controllers support with JSON configuration
		builder.Services.AddControllers()
			.AddJsonOptions(options =>
			{
				// Configure JSON serialization to handle cycles
				options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
				options.JsonSerializerOptions.WriteIndented = true;
			});

		// Configure HttpClient for server-side components
		builder.Services.AddHttpClient();
		builder.Services.AddScoped<HttpClient>(serviceProvider =>
		{
			var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient();

			// Configure base address for server-side components
			var context = serviceProvider.GetService<IHttpContextAccessor>();
			if (context?.HttpContext != null)
			{
				var request = context.HttpContext.Request;
				httpClient.BaseAddress = new Uri($"{request.Scheme}://{request.Host}");
			}
			else
			{
				// Fallback for development
				httpClient.BaseAddress = new Uri("https://localhost:7123/");
			}

			return httpClient;
		});

		builder.Services.AddHttpContextAccessor();

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
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddSignInManager()
			.AddDefaultTokenProviders();

		builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

		builder.Services.AddScoped<SchoolYearSupport>();

		// Configure Google Analytics options
		var googleAnalyticsOptions = builder.Configuration.GetSection("GoogleAnalytics").Get<GoogleAnalyticsOptions>() ?? new GoogleAnalyticsOptions();
		builder.Services.AddSingleton(googleAnalyticsOptions);

		// Register Cookie Consent and Google Analytics services  
		builder.Services.AddScoped<ICookieConsentService, CookieConsentService>();
		builder.Services.AddScoped<GoogleAnalyticsService>();

		builder.Services.AddCors(options =>
		{
			options.AddPolicy("AllowBlazorClient", policy =>
				policy.WithOrigins("https://localhost:7123",
								"https://luxfordpta.delfraisse.com")
					  .AllowAnyHeader()
					  .AllowAnyMethod());
		});

		var app = builder.Build();


		// Auto-run migrations on production startup (for FTP deployments)
		if (!app.Environment.IsDevelopment())
		{
			using (var scope = app.Services.CreateScope())
			{
				var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
				var markerFile = Path.Combine(env.ContentRootPath, "run-migration.txt");

				try
				{
					if (File.Exists(markerFile))
					{
						var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
						if (db != null)
						{
							// Enhanced logging for database connection diagnostics
							//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
							File.AppendAllText(markerFile, $"=== Migration Attempt Started at {DateTime.Now} ===\n");
							File.AppendAllText(markerFile, $"Environment: {app.Environment.EnvironmentName}\n");
							File.AppendAllText(markerFile, $"Connection String (masked): {MaskConnectionString(connectionString)}\n");

							// Test database connection with detailed error reporting
							try
							{
								File.AppendAllText(markerFile, "Testing database connection...\n");

								// Try to connect and get database info
								if (db.Database.CanConnect())
								{
									File.AppendAllText(markerFile, "✓ Database connection successful\n");

									// Get database information
									try
									{
										var dbName = db.Database.GetDbConnection().Database;

										// Fixed: Use SqlQueryRaw to get server version
										var versionResult = db.Database.SqlQueryRaw<string>("SELECT @@VERSION").AsEnumerable().FirstOrDefault();
										var serverVersion = versionResult ?? "Unknown";

										File.AppendAllText(markerFile, $"✓ Connected to database: {dbName}\n");
										File.AppendAllText(markerFile, $"✓ Server version: {serverVersion.Substring(0, Math.Min(100, serverVersion.Length))}...\n");
									}
									catch (Exception dbInfoEx)
									{
										File.AppendAllText(markerFile, $"⚠ Could not retrieve database info: {dbInfoEx.Message}\n");
									}

									// Attempt migration
									try
									{
										File.AppendAllText(markerFile, "Starting database migration...\n");
										db.Database.Migrate();

										// Run seeding after migration
										await SeedRolesAndAdminUser(scope.ServiceProvider);


										File.AppendAllText(markerFile, $"✓ Migration completed successfully at {DateTime.Now}\n");
										File.Move(markerFile, markerFile + ".success.log");
									}
									catch (Exception migrationEx)
									{
										File.AppendAllText(markerFile, $"✗ Migration failed: {migrationEx.Message}\n");
										if (migrationEx.InnerException != null)
										{
											File.AppendAllText(markerFile, $"✗ Migration inner exception: {migrationEx.InnerException.Message}\n");
										}
										File.AppendAllText(markerFile, $"✗ Migration stack trace: {migrationEx.StackTrace}\n");
										File.Move(markerFile, markerFile + ".migration-failed.log");
									}
								}
								else
								{
									File.AppendAllText(markerFile, $"✗ Database connection failed at {DateTime.Now}\n");

									// Try to get more specific connection error information
									try
									{
										await db.Database.OpenConnectionAsync();
										File.AppendAllText(markerFile, "✗ Unexpected: OpenConnectionAsync succeeded but CanConnect failed\n");
									}
									catch (Exception connEx)
									{
										File.AppendAllText(markerFile, $"✗ Connection error details: {connEx.Message}\n");
										if (connEx.InnerException != null)
										{
											File.AppendAllText(markerFile, $"✗ Connection inner exception: {connEx.InnerException.Message}\n");
										}

										// Log specific SQL Server error codes if available
										if (connEx is Microsoft.Data.SqlClient.SqlException sqlEx)
										{
											File.AppendAllText(markerFile, $"✗ SQL Error Number: {sqlEx.Number}\n");
											File.AppendAllText(markerFile, $"✗ SQL Error Severity: {sqlEx.State}\n");
											File.AppendAllText(markerFile, $"✗ SQL Error Server: {sqlEx.Server ?? "Unknown"}\n");
											File.AppendAllText(markerFile, $"✗ SQL Error Procedure: {sqlEx.Procedure ?? "N/A"}\n");
										}
									}

									// Additional diagnostic information
									File.AppendAllText(markerFile, $"✗ Current user context: {Environment.UserName}\n");
									File.AppendAllText(markerFile, $"✗ Machine name: {Environment.MachineName}\n");
									File.AppendAllText(markerFile, $"✗ Working directory: {Environment.CurrentDirectory}\n");
									File.AppendAllText(markerFile, $"✗ App base directory: {AppContext.BaseDirectory}\n");

									File.Move(markerFile, markerFile + ".connection-failed.log");
								}
							}
							catch (Exception testEx)
							{
								File.AppendAllText(markerFile, $"✗ Unexpected error during connection test: {testEx.Message}\n");
								File.AppendAllText(markerFile, $"✗ Test exception type: {testEx.GetType().Name}\n");
								if (testEx.InnerException != null)
								{
									File.AppendAllText(markerFile, $"✗ Test inner exception: {testEx.InnerException.Message}\n");
								}
								File.Move(markerFile, markerFile + ".test-failed.log");
							}
						}
						else
						{
							File.AppendAllText(markerFile, $"✗ Migration failed at {DateTime.Now}\n");
							File.AppendAllText(markerFile, $"✗ Error: Database context is null - dependency injection failed\n");
							File.AppendAllText(markerFile, $"✗ Service provider type: {scope.ServiceProvider.GetType().Name}\n");
							File.Move(markerFile, markerFile + ".context-null.log");
						}
					}
				}
				catch (Exception ex)
				{
					// Enhanced top-level error logging
					try
					{
						var errorFile = Path.Combine(env.ContentRootPath, $"migration-error-{DateTime.Now:yyyyMMdd-HHmmss}.log");
						File.WriteAllText(errorFile,
							$"=== Critical Migration Error at {DateTime.Now} ===\n" +
							$"Error: {ex.Message}\n" +
							$"Exception type: {ex.GetType().Name}\n" +
							$"Stack trace: {ex.StackTrace}\n");

						if (ex.InnerException != null)
						{
							File.AppendAllText(errorFile,
								$"Inner exception: {ex.InnerException.Message}\n" +
								$"Inner exception type: {ex.InnerException.GetType().Name}\n");
						}

						// Try to log connection string info if available
						try
						{
							//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
							File.AppendAllText(errorFile, $"Connection string (masked): {MaskConnectionString(connectionString)}\n");
						}
						catch
						{
							File.AppendAllText(errorFile, "Could not retrieve connection string\n");
						}
					}
					catch
					{
						// If we can't even write to an error log file, just continue
						// This prevents the application from failing to start due to logging issues
					}
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
			app.UseHsts();
		}

		app.UseHttpsRedirection();
		app.UseAntiforgery();

		app.MapStaticAssets();
		app.MapRazorComponents<App>()
			.AddInteractiveServerRenderMode()
			.AddInteractiveWebAssemblyRenderMode()
			.AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

		// Map controller endpoints
		app.MapControllers();
		app.MapAdditionalIdentityEndpoints();

		await app.RunAsync();
	}

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
			await userManager.CreateAsync(adminUser, "Admin123!");
			await userManager.AddToRoleAsync(adminUser, "Admin");
		}
	}

	static string MaskConnectionString(string? conn)
	{
		if (string.IsNullOrEmpty(conn)) return "";
		return System.Text.RegularExpressions.Regex.Replace(conn, @"Password\s*=\s*[^;]+", "Password=*****", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
	}
}
