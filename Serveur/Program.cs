using DotLiquid.FileSystems;
using DotLiquid;
using System.Threading;
using Server.Utils;
using Server.Database;
using Server.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(opt => {
    opt.ListenAnyIP(8080, opt => { });
    opt.ListenAnyIP(8443, opt =>
    {
        opt.UseHttps(
            /*
            System.Environment.GetEnvironmentVariable("CERTIFICATE_FILE") ?? "",
            System.Environment.GetEnvironmentVariable("CERTIFICATE_PASSWORD" ?? "")
            //*/
        );
    });
});

builder.Services.AddControllers();

builder.Services.AddAuthentication("Basic").AddScheme<AuthOptions, BasicAuthenticationHandler>("Basic", null);

var app = builder.Build();

Template.FileSystem = new LocalFileSystem(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "html", "partial"));

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Thread mailThread = new Thread(Email.SendMessages);
mailThread.IsBackground = true;
mailThread.Start();

app.Run();
