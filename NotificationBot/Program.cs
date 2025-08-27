var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

if (env.IsDevelopment())
{
	// Используем User Secrets
	builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.Run();
