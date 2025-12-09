using System.Net;
using System.Text;
using API;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Configuration;



var builder = WebApplication.CreateBuilder(args);


var config = builder.Configuration;

//Add JWT Validation
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!)),
        };

    });

builder.Services.AddSingleton<DynamicConfigProvider>();
builder.Services.AddSingleton<IProxyConfigProvider>(sp =>
    sp.GetRequiredService<DynamicConfigProvider>());

builder.Services.AddSingleton<YarpConfigurationHelper>();

builder.Services.AddReverseProxy();


var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    var tokenCookie = context.Request.Cookies["test_token"];
    var isCallback = path.StartsWithSegments("/auth/callback");
    var isAddRoute = path.StartsWithSegments("/add-route");

    if (isAddRoute)
    {
        await next();
        return;
    }
    if (tokenCookie is null && !isCallback)
    {
        var returnUrl = Uri.EscapeDataString($"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}");
        context.Response.Redirect($"http://10.11.1.190:5032/auth?returnUrl={returnUrl}");
        return;
    }

    if (tokenCookie is not null)
    {
        context.Request.Headers.Append("Authorization", $"Bearer {tokenCookie}");
    }
    await next();
});

app.MapGet("/auth/callback", async (context) =>
{
    var token = context.Request.Query["token"].ToString();
    var returnUrl = context.Request.Query["returnUrl"].ToString();
    if (string.IsNullOrWhiteSpace(token))
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        await context.Response.WriteAsync("Token is missing");
        return;
    }

    context.Response.Cookies.Append("test_token", token, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddHours(1)
    });

    context.Response.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
});

app.MapGet("health", () => "API is healthy");
app.MapPost("/add-route",
    (YarpConfigurationHelper yarp, [FromBody] AddRouteDto dto) =>
{
    yarp.AddApp(dto.AppName, dto.Host, dto.BackendAddress);
    return Results.Ok($"Added: {dto.AppName}");
});
app.MapReverseProxy();

app.UseHttpsRedirection();

app.Run();
