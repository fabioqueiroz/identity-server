using Duende.IdentityServer;
using Duende.IdentityServer.AspNetIdentity;
using IdentityServer.IdentityProvider.Data;
using IdentityServer.IdentityProvider.Models;
using IdentityServerAspNetIdentity.Pages.Admin.ApiScopes;
using IdentityServerAspNetIdentity.Pages.Admin.Clients;
using IdentityServerAspNetIdentity.Pages.Admin.IdentityScopes;
using IdentityServerAspNetIdentity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServer.IdentityProvider;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        //builder.Services.AddDbContext<ApplicationDbContext>(options =>
        //    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
            })
            //.AddInMemoryIdentityResources(Config.IdentityResources)
            //.AddInMemoryApiScopes(Config.ApiScopes)
            //.AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>()
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b =>
                    b.UseSqlServer(connectionString, dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b =>
                    b.UseSqlServer(connectionString, dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));

                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
                options.RemoveConsumedTokens = true;
            })
            .AddProfileService<ProfileService>();


        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
            });

        builder.Services.AddAuthorization(options =>
                options.AddPolicy("admin",
                    //policy => policy.RequireClaim("sub", "1"))
                    policy => policy.RequireClaim("role", "admin"))
            );

        builder.Services.Configure<RazorPagesOptions>(options =>
            options.Conventions.AuthorizeFolder("/Admin", "admin"));

        builder.Services.AddTransient<ClientRepository>();
        builder.Services.AddTransient<IdentityScopeRepository>();
        builder.Services.AddTransient<ApiScopeRepository>();

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();
        
        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}