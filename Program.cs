using ApiExamenCubos.Data;
using ApiExamenCubos.Helpers;
using ApiExamenCubos.Repositories;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using NSwag;
using NSwag.Generation.Processors.Security;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient
    (builder.Configuration.GetSection("KeyVault"));

});

SecretClient secretClient = builder.Services.BuildServiceProvider().GetService<SecretClient>();

KeyVaultSecret secret = await secretClient.GetSecretAsync("secretkey");
KeyVaultSecret audienceKey = await secretClient.GetSecretAsync("audience");
KeyVaultSecret issuerKey = await secretClient.GetSecretAsync("issuer");
KeyVaultSecret connectionKey = await secretClient.GetSecretAsync("connectionstring");

string secretKey = secret.Value;
string audience = audienceKey.Value;
string issuer = issuerKey.Value;
string connectionString = connectionKey.Value;

HelperActionServicesOAuth helper = new HelperActionServicesOAuth(issuer,audience,secretKey);

builder.Services.AddSingleton<HelperActionServicesOAuth>(helper);
//esta instancia del helper debemos incluirla dentro
//de nuestra app solamente una vez para que todo lo que
//hemos creado dentro no se genere de nuevo
builder.Services.AddSingleton<HelperActionServicesOAuth>(helper);
//habilitamos los servicios de auth que hemos creado 
//en el helper con action<>
builder.Services.AddAuthentication
    (helper.GetAuthenticateSchema())
    .AddJwtBearer(helper.GetJwtBearerOptions());

// Add services to the container.
builder.Services.AddTransient<RepositoryCubos>();
builder.Services.AddDbContext<CubosContext>
    (options => options.UseSqlServer(connectionString));


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "API COZY GAMES";
    document.Description = "API para CozyGames";
    document.AddSecurity("JWT", Enumerable.Empty<string>(),
        new NSwag.OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.ApiKey,
            Name = "Authorization",
            In = OpenApiSecurityApiKeyLocation.Header,
            Description = "Copia y pega el Token en el campo 'Value:' así: Bearer {Token JWT}."
        }
    );
    document.OperationProcessors.Add(
    new AspNetCoreOperationSecurityScopeProcessor("JWT"));
});

var app = builder.Build();
app.UseOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "CozyGames API");
    options.RoutePrefix = "";
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();