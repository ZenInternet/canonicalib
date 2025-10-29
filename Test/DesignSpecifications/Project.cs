using Zen.CanonicaLib.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCanonicaLib(() =>
    new Zen.CanonicaLib.UI.WebApplicationOptions()
    {
        PageTitle = "Zen's Canonical Libraries"
    }
);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCanonicaLib();

// Map controllers
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/canonicalib"));

app.Run();