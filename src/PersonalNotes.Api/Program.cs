using PersonalNotes.Api.Endpoints;
using PersonalNotes.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddPresentation(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonalNotes v1");
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseCors(PresentationExtensions.FrontCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthEndpoints();
app.MapFolderEndpoints();
app.MapDocumentEndpoints();
app.MapDashboardEndpoints();
app.MapBackupEndpoints();
app.MapAssistantEndpoints();

app.Run();
