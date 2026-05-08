using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TechAPI.DBContext;
using TechAPI.Middleware;
using TechAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== Add services =====
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// Performance
builder.Services.AddResponseCompression();
builder.Services.AddMemoryCache();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            ////policy
            ////    .WithOrigins("http://localhost:4200")
            ////    .AllowAnyHeader()
            ////    .AllowAnyMethod();

            policy.AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader();
        });
});

// Your services
builder.Services.AddScoped<IMiddlewareElementService, MiddlewareElement>();
builder.Services.AddScoped<RequestLoggingMiddleware>();
builder.Services.AddScoped<IDBContextService, DBContext>();
builder.Services.Scan(scan => scan
    .FromAssembliesOf(typeof(IUserService))
    .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Service")))
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);

// ===== Build app =====
var app = builder.Build();

// Response compression
app.UseResponseCompression();

// Swagger in development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = "swagger"; // important
    });
}

// ===== Middleware Pipeline =====

// 2️⃣ HTTPS & CORS
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");


// 1️⃣ Logging + JWT validation middleware (custom)
app.UseMiddleware<RequestLoggingMiddleware>();


// 3️⃣ Authentication/Authorization (still needed for [Authorize] attributes)
app.UseAuthentication();
app.UseAuthorization();

// Default redirect to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));


app.UseDefaultFiles();
app.UseStaticFiles();
// Map controllers
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");