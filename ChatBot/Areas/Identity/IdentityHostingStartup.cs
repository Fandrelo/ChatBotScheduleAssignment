using System;
using ChatBot.Areas.Identity.Data;
using ChatBot.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(ChatBot.Areas.Identity.IdentityHostingStartup))]
namespace ChatBot.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                /*services.AddDbContext<ChatBotContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("ChatBotContextConnection")));*/

                services.AddDefaultIdentity<ChatBotUser>()
                    .AddEntityFrameworkStores<ChatBotContext>();
            });
        }
    }
}