using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Security.Claims;
using Microsoft.Owin.Security.Cookies;
using System.Diagnostics;
//using System.IdentityModel.Claims

[assembly: OwinStartup(typeof(SmartDelivery.Startup))]

namespace SmartDelivery
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // inject Authorizatiion code inside pipline
            //context is an object hold recived object's information
            // async for making the function async in our case "next"
            app.Use(async (context, next) => {
                //Code here
                string UserName = context.Request.Query["userName"];
                int User_Type = Convert.ToInt32(Controllers.Business.GetLogin_UserType_Cookie());
                Debug.Print("My UserName In Startup class  " + UserName);
               // Debug.Print("User_Type In Startup class  " + User_Type);
                if (!string.IsNullOrEmpty(UserName) && UserName != "undefined" && UserName != "null")
                {
                    //identity  is an object from ClaimsIdentity  i will store user information on it 
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationType);
                    //Claim is a record
                    identity.AddClaim(new Claim(ClaimTypes.Name, UserName));

                    if (User_Type == 3)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "DeliverMen"));
                    }
                    if (User_Type == 4)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "NormalCustomers"));
                    }
                    if (User_Type == 5)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "Shopes"));
                    }

                    // fill user info 
                    context.Request.User = new ClaimsPrincipal(identity);
                }

                // next will call next function
                await next();
            });
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            app.MapSignalR();
        }
    }
}
