using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Boilerplate4Dev.Components;
using Boilerplate4Dev.Components.Account;
using Boilerplate4Dev.Data;
using Boilerplate4Dev.Data.Handlers;
using Boilerplate4Dev.Data.Helpers;
using Boilerplate4Dev.Data.Security;
using Boilerplate4Dev.Data.Services;
using Boilerplate4Dev.Data.Services.Private;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

// Add services to the container.
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

//Log Configuration
builder.Services.AddLogging(builder => builder.AddConsole());
builder.Services.AddOpenApi();

// Configure Sentry conditionally
if (!builder.Environment.IsDevelopment())
{
    // Configuração do Sentry
    builder.WebHost.UseSentry();
}

//Adiciona o serviço de cache e consulta de dados externos da aplicacao
builder.Services.AddMemoryCache();

// Configuração básica para Blazor Server .NET 9.0
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<IdentityService>();
builder.Services.AddScoped<IntegrationService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<GooglePeopleService>();
builder.Services.AddScoped<UserDashboardService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ICedulaCService, CedulaCService>();
builder.Services.AddScoped<IContraChequeService, ContraChequeService>();

// Configuração do Conexão com Banco de Dados Default
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));    

// Configuração do Identity
builder.Services.AddIdentity<ApplicationRoles, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSingleton<IEmailSender<ApplicationRoles>, IdentityNoOpEmailSender>();

builder.Services.AddAuthentication(options =>
{
    // Esquema padrão para o Blazor (cookies)
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Esquema de desafio para autenticação externa (Google)
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.SaveTokens = true;
    options.Scope.Add("https://www.googleapis.com/auth/user.organization.read");
    options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
    options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
    options.Scope.Add("https://www.googleapis.com/auth/directory.readonly");
    options.ClaimActions.MapJsonKey("image", "picture");
    options.Events = new OAuthEvents
    {
        OnTicketReceived = async context =>
        {
            var email = context.Principal.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var helper = context.HttpContext.RequestServices.GetRequiredService<GooglePeopleService>();
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationRoles>>();
                var user = await userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    var accessToken = context.Properties.GetTokenValue("access_token");
                    var person = await helper.GetGooglePersonInfo(accessToken);
                    var matricula = person.ExternalIds?.FirstOrDefault()?.Value;

                    await helper.ProcessOrganizationClaims(context.Principal, user, person);
                    await helper.ProcessUserClaims(context.Principal, user, accessToken, matricula);
                }
            }
        },
        OnCreatingTicket = async context =>
        {
            List<AuthenticationToken> tokens = context.Properties.GetTokens().ToList();
            tokens.Add(new AuthenticationToken() { Name = "TicketCreated", Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) });
            context.Properties.StoreTokens(tokens);
        }
    };
})
 .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidAudience = builder.Configuration["JwtBearerTokenSettings:Audience"],
            ValidIssuer = builder.Configuration["JwtBearerTokenSettings:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearerTokenSettings:SecretKey"]))
        };
    });

// Configuração de serviços adicionais
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddTransient<Boilerplate4Dev.Data.Handlers.TokenHandler>();

// Configuração de HTTP clients
builder.Services.AddHttpClient("HttpDefaultClientAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseUrl:ApiUrl"]);
})
.AddHttpMessageHandler<Boilerplate4Dev.Data.Handlers.TokenHandler>(); // Adiciona o TokenHandler

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("HttpDefaultClientAPI"));

builder.Services.AddHttpClient("GatewayAPI",
    httpClient =>
    {
        httpClient.BaseAddress = new Uri(builder.Configuration["BaseUrl:APIGatewayUrl"]);
        var basicAuthenticationValue =
                       Convert.ToBase64String(
                           Encoding.ASCII.GetBytes($"{builder.Configuration["Authentication:ApiGateway:Username"]}:{builder.Configuration["Authentication:ApiGateway:Password"]}"));
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", basicAuthenticationValue);
    });

// Configuração do BootstrapBlazor
builder.Services.AddServerSideBlazor();
builder.Services.AddBootstrapBlazor();
builder.Services.AddBootstrapBlazorTableExportService();
builder.Services.AddBootstrapBlazorHtml2PdfService();

// Configuração dos serviços de gerenciamento dos tokens Google e JWT
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<ITokenRenewalService, TokenRenewalService>();
builder.Services.AddHostedService<TokenRenewalBackgroundService>();

// Configuração de localização
builder.Services.AddLocalization();

// Add multi-language support configuration information
builder.Services.AddRequestLocalization<IOptionsMonitor<BootstrapBlazorOptions>>((localizerOption, blazorOption) =>
{
    localizerOption.DefaultRequestCulture = new RequestCulture("pt-BR");
    blazorOption.OnChange(op => Invoke(op));

    Invoke(blazorOption.CurrentValue);

    void Invoke(BootstrapBlazorOptions option)
    {
        var supportedCultures = option.GetSupportedCultures();
        localizerOption.SupportedCultures = supportedCultures;
        localizerOption.SupportedUICultures = supportedCultures;
    }
});

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Configuração de CORS
string? origins = "origins";
builder.Services.AddCors(options =>
{
    options.
        AddPolicy(
            origins,
            policy =>
            {
                policy
                .WithOrigins(builder.Configuration["BaseUrl:ApiUrl"])
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            }
        );
});

// Configuração de health checks
builder.Services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>()
                .AddCheck<ApiDependencyHealthCheck>("ApiDependencyCheck");

var app = builder.Build();

// Configuração do pipeline de requisições
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.MapOpenApi();
}

var option = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
if (option != null)
{
    app.UseRequestLocalization(option.Value);
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAsync(services);

    //Inicializa Servico de cache e consulta de dados externos da aplicacao
    var IntegrationService = scope.ServiceProvider.GetRequiredService<IntegrationService>();
    await IntegrationService.InitializeAsync();
}

app.UseCors(origins);
app.UseHttpsRedirection();
app.UseStaticFiles();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.UseBootstrapBlazor(); // ✅ Adiciona o middleware necessário

app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapHealthChecks($"{builder.Configuration["BaseUrl:BaseRouteIntegracaoApi"]}/healthcheck");

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

await app.RunAsync();
async Task SeedRolesAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roleNames = { "Administrador" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

