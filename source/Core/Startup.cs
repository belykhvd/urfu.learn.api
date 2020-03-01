using Contracts.Services;
using Contracts.Types.Solution;
using Contracts.Types.User;
using Core.Services;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Core
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //options => options.EnableEndpointRouting = false
            services.AddMvc();
            services.AddControllers();
            services.AddCors();

            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<ISolutionService, SolutionService>();

            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.AddTypeHandler(typeof(Profile), new DapperTypeHandler());
            SqlMapper.AddTypeHandler(typeof(Solution), new DapperTypeHandler());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
            );

            app.UseRouting();
            //app.UseMvc();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}