using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Api.Endpoints;
using Api.Data;
using Api.Repositories;
using Api.Services;
using Api.Models;
using System.Reflection;
using MediatR;
using Microsoft.FeatureManagement;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add Feature Management
builder.Services.AddFeatureManagement();

// Configure PaycheckCalculationOptions
builder.Services.Configure<PaycheckCalculationOptions>(
    builder.Configuration.GetSection(PaycheckCalculationOptions.SectionName));

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver")
    );
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("PaylocityBenefitsDb"));

// Register MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDependentRepository, DependentRepository>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register services (they now call CQRS)
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDependentService, DependentService>();
builder.Services.AddScoped<IPaycheckCalculationService, PaycheckCalculationService>();

// Register feature flag service
builder.Services.AddScoped<IFeatureFlagService, LocalFileAspFeatureFlagService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Employee Benefit Cost Calculation Api",
        Description = "API for employee benefit cost calculations"
    });

    // Custom document filter to replace version placeholders
    c.DocumentFilter<VersionSubstitutionDocumentFilter>();

    // Resolve schema conflicts for versioned APIs
    c.CustomSchemaIds(type => type.FullName);
});

var allowLocalhost = "allow localhost";
builder.Services.AddCors(options =>
{
    options.AddPolicy(allowLocalhost,
        policy => { policy.WithOrigins("http://localhost:3000", "http://localhost"); });
});

var app = builder.Build();

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var featureManager = app.Services.GetRequiredService<IFeatureManager>();
    if (await featureManager.IsEnabledAsync("EnableSwaggerUI"))
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            c.DefaultModelsExpandDepth(-1);
        });
    }
}

app.UseCors(allowLocalhost);

// Add feature flag middleware
app.UseMiddleware<Api.Middleware.FeatureFlagMiddleware>();

app.UseHttpsRedirection();

// Map V1 endpoints (now using CQRS + Repository Pattern with API Versioning)
app.MapDependentEndpointsV1();
app.MapEmployeeEndpointsV1();
app.MapFeatureFlagEndpointsV1();

app.Run();

// Make the Program class public for testing
public partial class Program { }
