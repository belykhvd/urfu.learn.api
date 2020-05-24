using Contracts.Services;
using Contracts.Types.Auth;
using Contracts.Types.Common;
using Contracts.Types.Course;
using Contracts.Types.Group;
using Contracts.Types.Solution;
using Contracts.Types.Task;
using Contracts.Types.User;
using Core.Repo;
using Core.Services;
using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
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
            services.AddCors(options => options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("http://localhost:4200")
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            }));

            services.AddMvc();
            services.AddControllers();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie();
            services.AddAuthorization();

            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IGroupService, GroupService>();
            services.AddSingleton<ICourseService, CourseService>();
            services.AddSingleton<ITaskService, TaskService>();
            services.AddSingleton<ISolutionService, SolutionService>();

            services.AddSingleton<ProfileRepo>();

            DefaultTypeMap.MatchNamesWithUnderscores = true;
            
            var dbStorableTypes = new[]
            {
                typeof(AuthResult),

                typeof(Profile),
                typeof(Group),

                typeof(Course),
                typeof(CourseIndex),

                typeof(CourseTask),
                typeof(Requirement),
                typeof(Requirement[]),
                typeof(TaskProgress),

                typeof(Solution),
                typeof(Link),
                typeof(Link[])
            };

            foreach (var type in dbStorableTypes)
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