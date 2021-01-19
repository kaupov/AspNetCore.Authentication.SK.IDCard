# AspNetCore.Authentication.SK.IdCard

**AspNetCore.Authentication.SK.IdCard** is a ID-card security middleware that you can use in your **ASP.NET Core** application to support SK ID-card authentication. It is inspired by **[Microsoft Certificate authentication](https://github.com/dotnet/aspnetcore/tree/master/src/Security/Authentication/Certificate)**. It is not perfect, but functional as external authentication.

**The first alpha release can be found on [NuGet](https://www.nuget.org/packages/AspNetCore.Authentication.SK.IdCard/1.0.0-alpha1)**.

## Getting started
**Install SK root CA and intermediate certificates to your running computer or server from [SK site](https://www.skidsolutions.eu/repositoorium/sk-sertifikaadid/)**. They have to be installed in propriate stores or received user certifiactes are not validated.

Authentication flow requires to redirect user to host that requires client certificate in TLS level. For that sample uses same application on different port, but it is also possible to use subdomain instead.

Configure your hosting environenment with additionally listen https with client certificate. Eg. add following lines to your `CreateHostBuilder`:
```AspNetCore
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
            webBuilder.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5000);
                options.ListenLocalhost(5001, listenOptions => listenOptions.UseHttps());
                options.ListenLocalhost(5002, listenOptions =>
                {
                    listenOptions.UseHttps(adapterOptions =>
                    {
                        adapterOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                    });
                });
            });
        });
```

Add following lines to your `Startup` class:
```AspNetCore
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication()
        .AddIdCard(options =>
        {
            options.MainSite = "https://localhost:5001";
            options.AuthenticationSite = "https://localhost:5002";
        })
}

public void Configure(IApplicationBuilder app)
{
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseIdCardAuthentication();
}
```
See the [/sample](https://github.com/kaupov/AspNetCore.Authentication.SK.IdCard/tree/main/sample) directory for a complete sample **using ASP.NET Core MVC**.