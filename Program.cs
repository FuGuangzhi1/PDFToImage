WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen().AddControllers();

WebApplication app = builder.Build();

app.UseSwagger().UseSwaggerUI();

app.MapControllers();

app.Run();
