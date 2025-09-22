using System.Security.Claims;
using System.Text.Json;
using LuxfordPTAWeb.Components.Account.Pages.Manage;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Routing
{
    internal static class IdentityComponentsEndpointRouteBuilderExtensions
    {
        // These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.
        public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            var accountGroup = endpoints.MapGroup("/Account");

            // External login endpoints are disabled - removing PerformExternalLogin endpoint

            accountGroup.MapPost("/Logout", async (
                HttpContext context,
                ClaimsPrincipal user,
                [FromServices] SignInManager<ApplicationUser> signInManager,
                [FromForm] string? returnUrl) =>
            {
                await signInManager.SignOutAsync();
                
                // Handle the returnUrl properly
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return TypedResults.LocalRedirect("~/");
                }

                // If returnUrl is already a full URI, extract the path
                if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
                {
                    returnUrl = uri.PathAndQuery;
                }

                // Ensure the returnUrl is safe and local
                if (!returnUrl.StartsWith("/"))
                {
                    returnUrl = "/" + returnUrl;
                }

                return TypedResults.LocalRedirect(returnUrl);
            });

            var manageGroup = accountGroup.MapGroup("/Manage").RequireAuthorization();

            // External login management is disabled - removing LinkExternalLogin endpoint

            var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var downloadLogger = loggerFactory.CreateLogger("DownloadPersonalData");

            manageGroup.MapPost("/DownloadPersonalData", async (
                HttpContext context,
                [FromServices] UserManager<ApplicationUser> userManager,
                [FromServices] AuthenticationStateProvider authenticationStateProvider) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user is null)
                {
                    return Results.NotFound($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
                }

                var userId = await userManager.GetUserIdAsync(user);
                downloadLogger.LogInformation("User with ID '{UserId}' asked for their personal data.", userId);

                // Only include personal data for download
                var personalData = new Dictionary<string, string>();
                var personalDataProps = typeof(ApplicationUser).GetProperties().Where(
                    prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
                foreach (var p in personalDataProps)
                {
                    personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
                }

                // External login data is disabled - removing external login references
                // var logins = await userManager.GetLoginsAsync(user);
                // foreach (var l in logins)
                // {
                //     personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
                // }

                personalData.Add("Authenticator Key", (await userManager.GetAuthenticatorKeyAsync(user))!);
                var fileBytes = JsonSerializer.SerializeToUtf8Bytes(personalData);

                context.Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");
                return TypedResults.File(fileBytes, contentType: "application/json", fileDownloadName: "PersonalData.json");
            });

            return accountGroup;
        }
    }
}
