using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChatBot.Data;
using ChatBot.Models;
using ChatBot.Services;
using Google.Cloud.Diagnostics.AspNetCore;
using Google.Cloud.Diagnostics.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatBot
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                //options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ChatBotContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("ChatBotContextConnection")));

            services.AddOptions();

            // Register the IConfiguration instance which MyOptions binds against.
            services.Configure<AWSOptions>(Configuration.GetSection("AWSConfiguration"));
            services.AddScoped<IAWSLexService, AWSLexService>();

            services.Configure<StackdriverOptions>(Configuration.GetSection("Stackdriver"));

            services.AddGoogleExceptionLogging(options =>
            {
                options.ProjectId = Configuration["Stackdriver:ProjectId"];
                options.ServiceName = Configuration["Stackdriver:ServiceName"];
                options.Version = Configuration["Stackdriver:Version"];
            });

            // Add trace service.
            services.AddGoogleTrace(options =>
            {
                options.ProjectId = Configuration["Stackdriver:ProjectId"];
                options.Options = Google.Cloud.Diagnostics.Common.TraceOptions.Create(
                    bufferOptions: BufferOptions.NoBuffer());
            });

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(300);
                options.Cookie.HttpOnly = true;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSignalR();
            services.AddSingleton<IEmailSender, EmailSender>(i => new EmailSender(Configuration["SignalRApiKey"]));
            services.AddSingleton<IStackDriverLogger, StackDriverLogger>(i => new StackDriverLogger(Configuration["Stackdriver:ProjectId"]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            loggerFactory.AddGoogle(Configuration["Stackdriver:ProjectId"]);
            var logger = loggerFactory.CreateLogger("testStackdriverLogging");
            logger.LogInformation("Stackdriver started. This is the startup log message.");
            logger.LogInformation("TEST-TEST", new AWSOptions[] { new AWSOptions {LexBotName = "TEST" } });
            IDictionary<string, string> entryLabels = new Dictionary<string, string>
            {
                { "size", "large" },
                { "color", "red" }
            };
            logger.LogInformation("TEST-TEST-LABELS", entryLabels);
            app.UseGoogleExceptionLogging();
            app.UseGoogleTrace();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseSession();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
