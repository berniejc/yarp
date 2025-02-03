
using System.Net;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false)
    .Build();
var password = "sherfieldRG270ED";
var campbellHouseCertificate = new X509Certificate2("../certificates/home.campbellfamily.co.uk.pfx",password);
var houseLocalCertificate = new X509Certificate2("../certificates/house.local.pfx",  password);
var interavoHouseCertificate = new X509Certificate2("../certificates/house.interavo.co.uk.pfx", password);
var interavoCertificate = new X509Certificate2("../certificates/interavo.co.uk.pfx",password);
var campbellCertificate = new X509Certificate2("../certificates/campbellfamily.co.uk.pfx",password);
builder.WebHost.UseKestrel(options =>
    {
        options.ListenAnyIP(80);
        if (campbellHouseCertificate != null)
        {
            options.Listen(IPAddress.Any, 443, listenOptions =>
            {
                listenOptions.UseHttps(httpsOptions =>
                    {
                        var certificates = new Dictionary<string, X509Certificate2>(
                            StringComparer.OrdinalIgnoreCase)
                        {
                            ["home.campbellfamily.co.uk"] = campbellHouseCertificate,
                            ["house.interavo.co.uk"]=interavoHouseCertificate,
                            ["interavo.co.uk"]=interavoCertificate,
                            ["www.interavo.co.uk"]=interavoCertificate,
                            ["test.campbellfamily.co.uk"]=interavoCertificate,
                            ["campbellfamily.co.uk"]=campbellCertificate,
                            ["www.campbellfamily.co.uk"]=campbellCertificate,
                            ["wrightnow.co.uk"]=campbellCertificate,
                            ["www.wrightnow.co.uk"]=campbellCertificate,
                            ["wildmoorwatsons.co.uk"]=campbellCertificate,
                            ["www.wildmoorwatsons.co.uk"]=campbellCertificate,
                            ["theharveys.co.uk"]=campbellCertificate,
                            ["www.theharveys.co.uk"]=campbellCertificate,
                            ["email-box.co.uk"]=campbellCertificate,
                            ["www.email-box.co.uk"] = campbellCertificate,
                            ["mail-to.co.uk"]=campbellCertificate,
                            ["www.mail-to.co.uk"]=campbellCertificate,
                            ["oakmeadow.co.uk"]=campbellCertificate,
                            ["www.oakmeadow.co.uk"]=campbellCertificate,
                            ["mysurname.co.uk"]=campbellCertificate,
                            ["www.mysurname.co.uk"]=campbellCertificate,
                            ["test.campbellfamily.co.uk"]=campbellCertificate,
                            ["smithsfamily.co.uk"]=campbellCertificate,
                            ["www.smithsfamily.co.uk"]=campbellCertificate
                        };
                        httpsOptions.ServerCertificateSelector = (context, host) =>
                        {
                            if (host is not null && certificates.TryGetValue(host, out var certificate))
                            {
                                return certificate;
                            }
                            return houseLocalCertificate;
                        };
                    }
                );
            });
        }
    });




builder.Services.AddReverseProxy()
    .LoadFromConfig(config.GetSection("ReverseProxy"));
builder.Services.AddHttpLogging(options => {});
    /*
    .ConfigureHttpClient((context, handler) =>
    {
        handler?.SslOptions?.ClientCertificates?.Add(houseLocalCertificate);
        handler?.SslOptions?.ClientCertificates?.Add(campbellCertificate);
    });
    */

var app = builder.Build();
app.UseHttpLogging();
app.UseRouting();
app.MapReverseProxy();
Console.WriteLine("about to run");
app.Run();