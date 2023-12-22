using Converter.Service;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (port is not null && int.TryParse(port, out var portNumber))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(portNumber);
    });
}

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IConverterService, ConverterService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
