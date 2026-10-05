using NhlDraftApp.Api.Drafts.Data;
using NhlDraftApp.Api.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddSingleton<IDraftFile>(services => new DraftFile(
    builder.Configuration["Draft:DataFolder"] ?? DraftFile.DefaultFolder,
    services.GetRequiredService<ILogger<DraftFile>>(),
    TimeProvider.System));
builder.Services.AddSingleton<DraftStore>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSameOriginWrites();
app.MapControllers();

app.Run();
