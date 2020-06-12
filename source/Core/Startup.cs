using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
using Contracts.Types.Course;
using Contracts.Types.Task;
using Core.JsonConverters;
using Core.Repo;
using Core.Services;
using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repo;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Core
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("http://localhost:4200")
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            }));

            services.AddMvc();
            services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(new GuidJsonConverter());
                        options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
                    });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.Events.OnRedirectToLogin = ctx =>
                        {
                            ctx.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                            return Task.CompletedTask;
                        };
                        options.Events.OnRedirectToAccessDenied = ctx =>
                        {
                            ctx.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                            return Task.CompletedTask;
                        };
                    });

            services.AddAuthorization();

            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IGroupService, GroupService>();
            services.AddSingleton<ICourseService, CourseService>();
            services.AddSingleton<ITaskService, TaskService>();

            services.AddSingleton<ProfileRepo>();
            services.AddSingleton<FileRepo>();

            DefaultTypeMap.MatchNamesWithUnderscores = true;

            var contractsAssembly = Assembly.GetAssembly(typeof(AuthData));
            foreach (var type in contractsAssembly.DefinedTypes)
            {
                if (type.IsClass && !type.ContainsGenericParameters)
                    SqlMapper.AddTypeHandler(type.AsType(), new DapperTypeHandler());
            }

            var arrayTypes = new[]
            {
                typeof(Requirement[]),
                typeof(RequirementStatus[])
            };

            foreach (var type in arrayTypes)
                SqlMapper.AddTypeHandler(type, new DapperTypeHandler());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseCors();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}