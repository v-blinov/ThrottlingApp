var builder = WebApplication.CreateBuilder(args);

//services

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseRouting();
app.UseEndpoints(routeBuilder =>
{
    routeBuilder.MapControllers();
});

app.Run();
