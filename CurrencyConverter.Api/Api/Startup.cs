using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OlegChibikov.OctetInterview.CurrencyConverter.Api.Data;
using OlegChibikov.OctetInterview.CurrencyConverter.Contracts;
using OlegChibikov.OctetInterview.CurrencyConverter.DataAccessLayer;

namespace OlegChibikov.OctetInterview.CurrencyConverter.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            services.AddControllers();
            services.AddHttpClient();
            services.AddSingleton<IConversionRateRepository, ConversionRateRepository>();

            services.AddSwaggerGen(c =>
                  {
                      c.SwaggerDoc("v1", new OpenApiInfo { Title = "CurrencyConverter.Api", Version = "v1" });
                  });
            services.Configure<Settings>(Configuration.GetSection("AppSettings"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = env ?? throw new ArgumentNullException(nameof(env));
            _ = app ?? throw new ArgumentNullException(nameof(app));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CurrencyConverter.Api v1"));
            }

            app.UseMiddleware<GlobalErrorHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
