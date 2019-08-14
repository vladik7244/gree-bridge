using System;
using Gree.API.Infrastructure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Gree.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var compositionRoot = new CompositionRoot();
            CreateWebHostBuilder(args, compositionRoot.ConfigureServices).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, Action<IServiceCollection> configureServices) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://0.0.0.0:5000")
                .ConfigureServices(configureServices)
                .UseStartup<Startup>();
    }
}