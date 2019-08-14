using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreeBlynkBridge;

namespace Gree.API.Infrastructure
{
    public class CompositionRoot
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConditionerService, GreeService>();
        }
    }
}
