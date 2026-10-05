using NhlDraftApp.Api.Drafts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddSingleton(new DraftStore(builder.Configuration["Draft:DataPath"] ?? DraftStore.DefaultPath));

var app = builder.Build();

app.UseExceptionHandler();
app.MapControllers();

app.Run();
