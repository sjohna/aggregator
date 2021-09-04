using AggregatorLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Aggregator
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        readonly string DatabaseFilePath = "Database/database.bin";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin()
                                             .AllowAnyMethod()
                                             .AllowAnyHeader();
                                  });
            });

            LiteDBFunctions.DoLiteDBGlobalSetUp();

            var databaseDirectory = Path.GetDirectoryName(DatabaseFilePath);
            Directory.CreateDirectory(databaseDirectory);   // TODO: error handling and logging

            var database = new LiteDB.LiteDatabase($"Filename={DatabaseFilePath}");
            var rawContentRepository = new LiteDBRawContentRepository(database);
            var unprocessedDocumentRepository = new LiteDBUnprocessedDocumentRepository(database);
            var system = new AggregatorSystem(rawContentRepository, unprocessedDocumentRepository);

            services.AddSingleton(typeof(AggregatorSystem), system);

            services
                .AddControllers()
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.Converters.Add(new InstantJsonConverter());
                    o.JsonSerializerOptions.Converters.Add(new UnprocessedDocumentContentJsonConverter());
                });

            services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.MaxDepth = 64;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Aggregator", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aggregator v1"));
            }

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
