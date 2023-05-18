using Microsoft.AspNetCore.Identity;

namespace BlazorTask;
public class SeedData
{
    public static async Task CreateDefaultRoles(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<SeedData>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
     
        try
        {
            logger.LogInformation("Begined Adding default roles");
            var roles = config.GetSection("Identity:Roles").Get<List<string>>();
            
            foreach (var role in roles!)
            {
                logger.LogInformation("This role "+ role);
                try
                {
                if( await roleManager.FindByNameAsync(role) is null)
                await roleManager.CreateAsync(new IdentityRole(role));

                logger.LogInformation("Role created with name "+ role);
                }
                catch(Exception e)
                {
                logger.LogInformation("Failed Creating role with name "+ role);
                throw new Exception(e.Message);
                }
            }
        
        logger.LogInformation("Ended creating roles");
        roleManager.Dispose();
            
        }
        catch (System.Exception e)
        {
            
            throw new Exception(e.Message);
        }
    }

    public static async Task CreateDefaultUser(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<SeedData>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var admin = config.GetSection("Identity:User").Get<SeedUser>();
        
        var newAdmin = new IdentityUser
        {
           UserName = admin!.UserName,
           Email = admin.Email
        };

        if(newAdmin is null)
        {
            logger.LogInformation("User is not found frim apsettings");
            return;
        }
        
        try
        {
            var isExistAdmin = await userManager.CheckPasswordAsync(newAdmin, admin.Password!);
            var role = await roleManager.FindByNameAsync("User");

            if(!isExistAdmin && role is not null)
            {
                var createAdminResult =  await userManager.CreateAsync(newAdmin, admin.Password!);

                if(!createAdminResult.Succeeded)
                {
                    logger.LogInformation("User is not created with name "+  admin.UserName);
                    return;
                }

                var addToRoleResult = await userManager.AddToRoleAsync(newAdmin, role.Name!);

                if(!addToRoleResult.Succeeded)
                {
                    logger.LogInformation("Role is not added with name "+ role.Name);
                    return;
                }
            }
            
            logger.LogInformation("User Created Successefuly with name "+ admin.UserName);
        }
        catch(Exception e)
        {
           logger.LogInformation("Error with creating Seed User");
           throw new Exception(e.Message);
        }
    }
  }

public class SeedUser
{
    public string? UserName {get; set;}
    public string? Password {get; set;}
    public string? Email {get; set;}
}