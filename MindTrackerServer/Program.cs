using BLL.DI;
using MindTrackerServer.MiddleWares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

DependencyContainer.RegisterDependency(builder.Services, builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ErrorHandlerMiddleWare>();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
