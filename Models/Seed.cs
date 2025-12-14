using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OAuthServer.Models
{
    public static class Seed
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            if (await manager.FindByClientIdAsync("spa-client") is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "spa-client",
                    ClientType = ClientTypes.Public,
                    DisplayName = "Vue SPA",
                    PostLogoutRedirectUris =
                {
                    new Uri("http://localhost:8080/")
                },
                    RedirectUris =
                {
                    new Uri("http://localhost:8080/callback")
                },

                    Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,

                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,

                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    "api"
                },
                    Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
                });
            }
        }
    }
}
