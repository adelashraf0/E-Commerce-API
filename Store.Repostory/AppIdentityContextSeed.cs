using Microsoft.AspNetCore.Identity;
using Store.Data.Entities.IdentityEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Repository
{
    public class AppIdentityContextSeed
    {
        public static async Task SeedUserAsync(UserManager<AppUser> userManager)
        {
            if (!userManager.Users.Any()) 
            {
                var user = new AppUser
                {
                    DisplayName = "Adel Ashraf",
                    Email = "adelashraf@gmail.com",
                    UserName = "adelashraf",
                    Address = new Address
                    {
                        FirstName = "Adel",
                        LastName = "Ashraf",
                        City = "Maadi",
                        State = "Cairo",
                        Street = "44",
                        ZipCode = "12355"
                    }
                };

                await userManager.CreateAsync(user, "Password123!");
            }
        }
    }
}
