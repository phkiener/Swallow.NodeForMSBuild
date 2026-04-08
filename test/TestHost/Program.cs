using Microsoft.AspNetCore.Hosting.StaticWebAssets;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration);
app.MapStaticAssets();

app.Run();
