using IdentityServer4;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace IdentityServer4Demo
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            
            // cookie policy to deal with temporary browser incompatibilities
            services.AddSameSiteCookiePolicy();

            services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddInMemoryApiScopes(Config.GetApiScopes())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryClients(Config.GetClients())
                .AddTestUsers(TestUsers.Users)
                .AddDeveloperSigningCredential(persistKey: false);

            services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = Configuration["Secret:GoogleClientId"];
                    options.ClientSecret = Configuration["Secret:GoogleClientSecret"];
                })
                .AddOpenIdConnect("aad", "Sign-in with Azure AD", options =>
                {
                    options.Authority = "https://login.microsoftonline.com/common";
                    options.ClientId = "https://leastprivilegelabs.onmicrosoft.com/38196330-e766-4051-ad10-14596c7e97d3";

                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                    options.ResponseType = "id_token";
                    options.CallbackPath = "/signin-aad";
                    options.SignedOutCallbackPath = "/signout-callback-aad";
                    options.RemoteSignOutPath = "/signout-aad";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidAudience = "165b99fd-195f-4d93-a111-3e679246e6a9",

                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                })
                .AddLocalApi(options =>
                {
                    options.ExpectedScope = "api";
                });

            // preserve OIDC state in cache (solves problems with AAD and URL lenghts)
            services.AddOidcStateDataFormatterCache("aad");

            // add CORS policy for non-IdentityServer endpoints
            services.AddCors(options =>
            {
                options.AddPolicy("api", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            // demo versions (never use in production)
            services.AddTransient<IRedirectUriValidator, DemoRedirectValidator>();
            services.AddTransient<ICorsPolicyService, DemoCorsPolicy>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCookiePolicy();
            app.UseSerilogRequestLogging();
            app.UseDeveloperExceptionPage();

            app.UseCors("api");

            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}