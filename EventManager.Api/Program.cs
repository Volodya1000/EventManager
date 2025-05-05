using EventManager.Api.Extensions;
using EventManager.API.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel();

builder.Services.AddSwaggerConfiguration();
builder.Services.AddIdentityConfiguration();
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddCacheConfiguration(builder.Configuration);
builder.Services.AddAutoMapperConfiguration();
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddValidation();
builder.Services.AddAuthConfiguration(builder.Configuration);

builder.Services.AddAntiforgery();
builder.Services.AddHttpContextAccessor();

builder.Services.AddApiAuthentication(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();    
    app.MapSwagger();    
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "EventManager");
    });
    app.ApplyMigrations();
}

app.UseExceptionHandler(_ => { });
app.ConfigureStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.AddMappedEndpoints();

app.Run();


