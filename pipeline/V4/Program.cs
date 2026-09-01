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
using Boilerplate4Dev.Data.Services.Private.PosGraduacao;
using Boilerplate4Dev.Data.Services.Private.RelatorioFerias;
using Boilerplate4Dev.Data.Services.Private.RelatorioSubLotacao;
using Boilerplate4Dev.Data.Services.Private.RelatorioAusencias;
using Boilerplate4Dev.Data.Services.Private.RelatorioLicensa;
using Boilerplate4Dev.Data.Services.Private.RelatorioTempoServico;
using Boilerplate4Dev.Data.Services.Private.CessaoService;
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
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IdentityService>();
builder.Services.AddScoped<IntegrationService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<GooglePeopleService>();
builder.Services.AddScoped<UserDashboardService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ICedulaCService, CedulaCService>();
builder.Services.AddScoped<IContraChequeService, ContraChequeService>();
builder.Services.AddScoped<IContraChequePensaoService, ContraChequePensaoService>();
builder.Services.AddScoped<IRelFeriasService, RelFeriasService>();
builder.Services.AddScoped<IHistSubLotacaoService, HistSubLotacaoService>();
builder.Services.AddScoped<IHistLotacaoService, HistLotacaoService>();
builder.Services.AddScoped<IRelAusenciasService, RelAusenciasService>();
builder.Services.AddScoped<IRelLicensaService, RelLicensaService>();
builder.Services.AddScoped<EmpregadoService>();
builder.Services.AddScoped<IDadosCadastraisService, DadosCadastraisService>();
builder.Services.AddScoped<IAfastaTemporarioService, AfastaTemporarioService>();
builder.Services.AddScoped<IFichaFinanceiraService, FichaFinanceiraService>();
builder.Services.AddScoped<FuncaoGratificadaService>();
builder.Services.AddScoped<ElogioPunicaoService>();

builder.Services.AddScoped<IHistoricoCargosService, HistoricoCargosService>();
builder.Services.AddScoped<IDadosCadastraisService, DadosCadastraisService>();
builder.Services.AddScoped<IAfastaTemporarioService, AfastaTemporarioService>();
builder.Services.AddScoped<FuncaoGratificadaService>();

builder.Services.AddScoped<IRelFeriasService, RelFeriasService>();
builder.Services.AddScoped<IHistSubLotacaoService, HistSubLotacaoService>();
builder.Services.AddScoped<IHistLotacaoService, HistLotacaoService>();
builder.Services.AddScoped<IRelAusenciasService, RelAusenciasService>();
builder.Services.AddScoped<IRelLicensaService, RelLicensaService>();
builder.Services.AddScoped<IRelTempoServicoService, RelTempoServico>();
builder.Services.AddScoped<EmpregadoService>();
builder.Services.AddScoped<DadosFuncionaisService>();
builder.Services.AddScoped<IPosGraduacaoService, PosGraduacaoService>();
builder.Services.AddScoped<PosGraduacaoContextService>();
builder.Services.AddScoped<ICessaoService, CessaoService>();

builder.Services.AddScoped<IAcademicoService, AcademicoService>();

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
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("GoogleOAuth");
                var user = await userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    var accessToken = context.Properties.GetTokenValue("access_token");
                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        try
                        {
                            var person = await helper.GetGooglePersonInfo(accessToken);
                            var matricula = person.ExternalIds?.FirstOrDefault()?.Value;

                            await helper.ProcessOrganizationClaims(context.Principal, user, person);
                            await helper.ProcessUserClaims(context.Principal, user, accessToken, matricula);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Falha ao enriquecer claims do usuário {Email} no callback OAuth do Google.", email);
                        }
                    }
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
builder.Services.AddScoped<IFileDownloadService, FileDownloadService>();
builder.Services.AddScoped<ITxtExportService, TxtExportService>();

// Configuração dos serviços de gerenciamento dos tokens Google e JWT
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<ITokenRenewalService, TokenRenewalService>();

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
     // Limpa as redes/proxies conhecidos para confiar no proxy reverso independente do IP
    // (necessário em ambientes Docker onde o IP do proxy não é loopback)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
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
    //Inicializa Servico de cache e consulta de dados externos da aplicacao
    var IntegrationService = scope.ServiceProvider.GetRequiredService<IntegrationService>();
    await IntegrationService.InitializeAsync();
}

await EnsureIdentityBootstrapAsync(app.Services);

// Deve ser chamado antes de UseAuthentication para que o scheme seja corrigido
// para https quando o proxy reverso envia X-Forwarded-Proto: https
app.UseForwardedHeaders();

app.UseCors(origins);
app.UseHttpsRedirection();
app.UseStaticFiles();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.UseBootstrapBlazor(); 

app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapHealthChecks($"{builder.Configuration["BaseUrl:BaseRouteIntegracaoApi"]}/healthcheck");

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

await app.RunAsync();

static async Task EnsureIdentityBootstrapAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("IdentityBootstrap");
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationRoles>>();

    var requiredRoles = new[]
    {
        ApplicationRoleNames.Administrator,
        ApplicationRoleNames.SirhAdm,
        ApplicationRoleNames.SirhGestor,
        ApplicationRoleNames.Empregado
    };

    foreach (var roleName in requiredRoles)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            continue;
        }

        var createRoleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        if (!createRoleResult.Succeeded && !await roleManager.RoleExistsAsync(roleName))
        {
            var errors = string.Join(", ", createRoleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Falha ao criar a role '{roleName}': {errors}");
        }

        logger.LogInformation("Role {RoleName} criada durante o bootstrap da aplicacao.", roleName);
    }

    var adminUsers = await userManager.GetUsersInRoleAsync(ApplicationRoleNames.Administrator);
    if (adminUsers.Count > 0)
    {
        return;
    }

    var totalUsers = await userManager.Users.CountAsync();
    if (totalUsers != 1)
    {
        if (totalUsers > 1)
        {
            logger.LogWarning(
                "Nenhum usuario possui a role {RoleName}, mas existem {UserCount} usuarios cadastrados. Bootstrap automatico nao aplicou privilegio administrativo.",
                ApplicationRoleNames.Administrator,
                totalUsers);
        }

        return;
    }

    var onlyUser = await userManager.Users.SingleAsync();
    if (await userManager.IsInRoleAsync(onlyUser, ApplicationRoleNames.Administrator))
    {
        return;
    }

    var addRoleResult = await userManager.AddToRoleAsync(onlyUser, ApplicationRoleNames.Administrator);
    if (!addRoleResult.Succeeded)
    {
        var errors = string.Join(", ", addRoleResult.Errors.Select(error => error.Description));
        throw new InvalidOperationException(
            $"Falha ao vincular o usuario {onlyUser.Email} a role '{ApplicationRoleNames.Administrator}': {errors}");
    }

    await userManager.UpdateSecurityStampAsync(onlyUser);
    logger.LogInformation(
        "Usuario {Email} vinculado automaticamente a role {RoleName} durante o bootstrap da aplicacao.",
        onlyUser.Email,
        ApplicationRoleNames.Administrator);
}
