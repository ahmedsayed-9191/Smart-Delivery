using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartDelivery.Models;
using Microsoft.AspNet.SignalR;
using SmartDelivery.HUBs;
using System.Data.SqlClient;
using System.Diagnostics;

namespace SmartDelivery.Controllers
{
    public class HomeController : Controller
    {
        private SmartDeliveryEntities db = new SmartDeliveryEntities(); 
        public static Boolean Authenticated = false;
 
        // GET: Home
        public ActionResult Index()
        {
            ViewBag.UserType = new SelectList(db.UserTypes.SqlQuery("select * from UserType Where ID != 1"), "ID" , "User Type");

            return View();
        }


        //Login
        [HttpPost]
        public ActionResult LogIn([Bind(Include = "UserName,PassWord")] Employee user )
        {
            Authenticated = true;
            Debug.WriteLine("user.username "+user.UserName + "user.paddword : "+ user.PassWord);
            Debug.WriteLine("ؤontroler updated");

            string username = user.UserName;
            string password = Encryption.Encrypt(user.PassWord);
           
            var myUser = db.Employees.FirstOrDefault(u => u.UserName == username && u.PassWord == password);

            if (myUser != null)
            {
                int day = ((int)DateTime.Now.DayOfWeek) + 2; // we add 2 to make the time consistent

                if (myUser.Authorized == 0) //Check the user is blocked or not
                {
                    Response.Write("<script>alert('This User Is blocked !');</script>");
                    TempData["msg10"] = "<script>alert('This User Is blocked !');</script>";
                    return RedirectToAction("Index");
                }
                
                
               else if(myUser.HolidayID == day)
                {
                    Response.Write("<script>alert('This is Your Holiday! Have a Nice Time.');</script>");
                    TempData["msg10"] = "<script>alert('This is Your Holiday! Have a Nice Time.');</script>";
                    return RedirectToAction("Index");
                }

               
                else
                {
                    // make Delivery Man Active
                    int result = db.Database.ExecuteSqlCommand("Update  [dbo].[Employee] SET Active = 1 where ID = @id", new SqlParameter("@id", myUser.ID));
                    Debug.WriteLine("Employee IS ACTIVE ? => " + result);
                    if (myUser.EmployeeType == 1)
                {
                    return RedirectToAction("Index", "Admin", new { id = myUser.ID , usertype = myUser.EmployeeType });
                }
                else if (myUser.EmployeeType == 2)
                {
                    return RedirectToAction("Index", "SuperVisor", new { id = myUser.ID, usertype = myUser.EmployeeType });
                }
                else if (myUser.EmployeeType == 3)
                {
                    
                    return RedirectToAction("Index", "DeliveryMan", new { id = myUser.ID, usertype = myUser.EmployeeType });
                }
                }
            }
            else
            {
                var myUser2 = db.Customers.FirstOrDefault(u => u.UserName == username && u.PassWord == password);

                if (myUser2 != null)
                {
                    //Check the user is blocked or not
                    if (myUser2.Authorized == 0)
                    {
                        Response.Write("<script>alert('This User Is blocked !');</script>");
                        TempData["msg10"] = "<script>alert('This User Is blocked !');</script>";
                        return RedirectToAction("Index");
                    }

                    if (myUser2.CustomerType == 4)
                    {
                        return RedirectToAction("Index", "NormalCustomer", new { id = myUser2.ID, usertype = myUser2.CustomerType });
                    }
                    else if (myUser2.CustomerType == 5)
                    {
                        return RedirectToAction("Index", "Shop", new { id = myUser2.ID, usertype = myUser2.CustomerType });
                    }
                        

                }

            }

            Response.Write("<script>alert('UserName Or Password  Incorrect.');</script>");
            TempData["msg10"] = "<script>alert('UserName Or Password  Incorrect !');</script>";

            return RedirectToAction("Index");
        }

        //Log out 
        public ActionResult LogOut()
        {
            Authenticated = false;
            if (Session["id"] != null)
            {
                int id = int.Parse(Session["id"].ToString());
                var user = db.Employees.Find(id);
                try
                {
                    if (user != null)
                    {
                        user.Active = 0;

                        db.SaveChanges();

                    }
                    else
                    {
                        var customer = db.Customers.Find(id);
                        customer.Active = 0;
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error In save changes." + ex.Message);
                }
            }


            var hubContext = GlobalHost.ConnectionManager.GetHubContext<SmartDeliveryHub>();
            string My_User_Name = "";
            if (Session["userName"] != null)
            {
                My_User_Name = Session["userName"].ToString();
            }
            try
            {
                hubContext.Clients.Group(My_User_Name).logoff(); //call Client().logoff(); this will end the exception
            }
            catch { }

            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();

            return RedirectToAction("Index", "home");
        }

        //Get
        public ActionResult Register()
        {
            //List Of UserTypes shop and normal customer
            ViewBag.CustomerType = new SelectList(db.UserTypes.Where(a=> a.UserType1 == "NormalCustomer" 
                                                                  || a.UserType1 == "Shop").ToList(),"ID" , "UserType1");

            return View();
        }
         
        [HttpPost]
        public ActionResult Register(Customer customer, HttpPostedFileBase image)
        {
            //List Of UserTypes shop and normal customer
            ViewBag.CustomerType = new SelectList(db.UserTypes.Where(a => a.UserType1 == "NormalCustomer"
                                                                  || a.UserType1 == "Shop").ToList(), "ID", "UserType1");

            if (image != null)
            {
                //Save image 
                customer.Photo = new byte[image.ContentLength];
                image.InputStream.Read(customer.Photo, 0, image.ContentLength);

                //check the username of the customer if exits or not 
                Customer checkCustomerExistInCustomers = new Customer();
                checkCustomerExistInCustomers = db.Customers.FirstOrDefault(cus => cus.UserName == customer.UserName);

                Employee checkCustomerExistInEmployees = new Employee();
                checkCustomerExistInEmployees = db.Employees.FirstOrDefault(cus => cus.UserName == customer.UserName);
                if (checkCustomerExistInCustomers != null || checkCustomerExistInEmployees != null)
                {
                    Response.Write("<script>alert('This UserName Is Not Available Type Another One !.');</script>");
                    return View(customer);
                }
                //check the username of the customer if exits or not 
                Customer checkCustomerExistInCustomersEmailes = new Customer();
                checkCustomerExistInCustomersEmailes = db.Customers.FirstOrDefault(cus => cus.Email == customer.Email);

                Employee checkCustomerExistInEmployeesEmails = new Employee();
                checkCustomerExistInEmployeesEmails = db.Employees.FirstOrDefault(cus => cus.Email == customer.Email);
                if (checkCustomerExistInCustomersEmailes != null || checkCustomerExistInEmployeesEmails != null)
                {
                    Response.Write("<script>alert('This Email alreedy have an account!.');</script>");
                    return View(customer);
                }
                //add customer to database
                Customer myCustomer = new Customer();
                myCustomer.FirstName = customer.FirstName;
                myCustomer.LastName = customer.LastName;
                myCustomer.Email = customer.Email;
                myCustomer.Phone = customer.Phone;
                myCustomer.Address = customer.Address;
                myCustomer.Photo = customer.Photo;
                myCustomer.UserName = customer.UserName;
                myCustomer.PassWord =Encryption.Encrypt(customer.PassWord);
                myCustomer.Active = 1;
                myCustomer.Authorized = 1;
                myCustomer.CustomerType = customer.CustomerType;
                myCustomer.ShopName = customer.ShopName;

                db.Customers.Add(myCustomer);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('invalid Data! Try again.');</script>");
                    return View(customer);
                }
                Response.Write("<script>alert('" + customer.FirstName + " " + customer.LastName + " Registered Successfully.');</script>");
                TempData["msg10"] = "<script>alert('" + customer.FirstName + " " + customer.LastName + " Registerd Successfully.');</script>";


                return RedirectToAction("Index");
            }
            else
            {
                //Check image is submitted or not 
                if (image == null)
                {
                    Response.Write("<script>alert('Select Photo Please.');</script>");
                }

                return View(customer);

            }
        }
    }
}