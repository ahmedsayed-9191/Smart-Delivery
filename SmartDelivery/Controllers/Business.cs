using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace SmartDelivery.Controllers
{
    public class Business
    {

        public static void SetCookie(string Id, string usertype, string CookieName = "Id")
        {
            HttpCookie myCookie = new HttpCookie("Login");
            Debug.Print("User_ID Is In SetCookie : " + Id);
            Debug.Print("User_Type Is In SetCookie : " + usertype);
            if (myCookie.Values["LoginId"] == null)
            {
                myCookie.Values.Add("LoginId", Id);
            }

            if (myCookie.Values["LoginUserType"] == null)
            {

                myCookie.Values.Add("LoginUserType", usertype);
            }

            myCookie.Expires = DateTime.Now.AddMonths(6);

            HttpContext.Current.Response.Cookies.Add(myCookie);

        }
        public static string GetLoginCookie(string CookieName = "Login")
        {
            HttpCookie myCookie = HttpContext.Current.Request.Cookies[CookieName];
            if (myCookie == null)
            {
                return "-1";
            }
            else
            {
                return myCookie.Values["LoginId"];
            }
        }
        public static string GetLogin_UserType_Cookie(string CookieName = "Login")
        {
            HttpCookie myCookie = HttpContext.Current.Request.Cookies[CookieName];
            if (myCookie == null)
            {
                return "-1";
            }
            else
            {
                return myCookie.Values["LoginUserType"];
            }
        }
    }
}