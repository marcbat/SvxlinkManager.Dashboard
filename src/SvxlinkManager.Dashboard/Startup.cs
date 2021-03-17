using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SvxlinkManager.Common.Models;
using SvxlinkManager.Common.Service;
using SvxlinkManager.Dashboard.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SvxlinkManager.Dashboard
{
  public class Startup
  {
    private readonly string StreamingPoliciyName = "Streaming";

    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCors(o =>
      {
        o.AddPolicy(StreamingPoliciyName, b => b.WithOrigins("http://salonsuisseromand.northeurope.cloudapp.azure.com:8000"));
      });

      services.AddRazorPages();
      services.AddServerSideBlazor();
      services.AddSingleton<SvxlinkServiceBase>();
      services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
      }

      using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
      {
        var applicationPath = Directory.GetCurrentDirectory();
        var channel = new ChannelBase { Name = "Salon Suisse Romand" };

        var svxlinkservice = serviceScope.ServiceProvider.GetRequiredService<SvxlinkServiceBase>();

        svxlinkservice.StartSvxlink(channel, pidFile: "/var/run/svxlink.pid", runAs: "root", configFile: $"{applicationPath}/SvxlinkConfig/svxlink.conf");
      }

      app.UseStaticFiles();

      app.UseRouting();

      app.UseCors(StreamingPoliciyName);

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapBlazorHub();
        endpoints.MapFallbackToPage("/_Host");
      });
    }
  }
}