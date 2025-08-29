
using WolServer.Models;
using WolServer.Serialization;
using WolServer.Services;
using Microsoft.OpenApi.Models;
using WOL_Server.Helper;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

builder.Services.AddSingleton<IUdpSender, UdpSender>();
builder.Services.AddTransient<IWolService, WakeOnLanService>();
builder.Services.AddTransient<RunCommand>();
builder.Services.AddAuthorization();

builder = AuthHelper.SetAuthentication(builder);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WOL Server API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WOL Server API v1"));
}

var toolApi = app.MapGroup("/wol");
toolApi.RequireAuthorization();
toolApi.MapPost("/", (RunCommand command, WakeOnLanRequest request) =>
{
    if (request == null || string.IsNullOrWhiteSpace(request.MacAddress))
        return Results.BadRequest(new WakeOnLanResponse(false));

    var mac = request.MacAddress;
    return command.SendWakeOnLan(mac)
        ? Results.Ok(new WakeOnLanResponse(true))
        : Results.BadRequest(new WakeOnLanResponse(false));
        
}).RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.Run();


