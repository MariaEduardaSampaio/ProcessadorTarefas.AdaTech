using Application.Services.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection ApplicationDependencyInjection(this IServiceCollection services)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string configurationPath = Path.Combine(basePath, "AppSettings.json").Replace("\\bin\\Debug\\net8.0", "");

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(configurationPath, optional: true, reloadOnChange: true)
                .Build();
            services.AddScoped(_ => config);

            services.AddSingleton<IRepository<Tarefa>, TaskRepository>();
            services.AddScoped<IProcessadorTarefas, ProcessadorTarefas>();
            services.AddScoped<IGerenciadorTarefas, GerenciadorTarefas>();

            return services;
        }
    }
}
