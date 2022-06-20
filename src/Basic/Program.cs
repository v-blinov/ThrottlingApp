using Basic.Extensions;
using Basic.Optional;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));

builder.Services.AddControllers();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

// needs after app.UseRouting(); if that exists
app.UseRateLimiting();

app.MapControllers();

app.Run();
