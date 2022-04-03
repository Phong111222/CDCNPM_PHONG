
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using CDCNPM.Repositories;
using Microsoft.Extensions.FileProviders;
using System.IO;
using DevExpress.AspNetCore;

namespace CDCNPM
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940


        public IConfiguration configuration { get; }

        public Startup(IConfiguration theConfiguration)
        {
            this.configuration = theConfiguration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options=>options.EnableEndpointRouting = false);

            services.AddSingleton<ISqlTableRepository,SqlTableRepositoryImpl>();
            services.AddDevExpressControls();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "node_modules")),
                RequestPath = "/node_modules"
            });
            app.UseStaticFiles();
            app.UseDevExpressControls();
            app.UseMvc();

        }
    }
}
