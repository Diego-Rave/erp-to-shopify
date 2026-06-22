using ErpToShopify.Interfaces;
using ErpToShopify.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
     
        //services.AddScoped<IBnetService, BnetSqlService>();
        // Usamos AddHttpClient porque este servicio necesita hacer peticiones web a FastAPI
        services.AddHttpClient<IBnetService, BnetApiService>(client =>
        {
            // Lee la URL base desde tu local.settings.json (o usa localhost por defecto en pruebas)
            var bnetApiUrl = Environment.GetEnvironmentVariable("BnetApiBaseUrl") ?? "http://localhost:8000";
            client.BaseAddress = new Uri(bnetApiUrl);
        });


        // 2. Registramos el servicio de Shopify junto con su cliente HTTP
        services.AddHttpClient<IShopifyService, ShopifyService>();
    })
    .Build();

host.Run();
