using SmartDelivery.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartDelivery.Controllers
{
    public class DeliveryManController : Controller
    {

        SmartDeliveryEntities db = new SmartDeliveryEntities();

        // GET: DeliveryMan
        public ActionResult Index(int? id, int? usertype)
        {
            if (!HomeController.Authenticated && usertype != 3)
            {
                return RedirectToAction("Index", "home");
            }

            if (id != null)
            {
                Employee employee = db.Employees.Find(id);
                Session["userName"] = employee.UserName;
                Session["userType"] = employee.UserType.ToString();
                Session["id"] = id;
                //set cookie to can access from hubs
                Business.SetCookie(id.ToString(), employee.EmployeeType.ToString());
                //List all Delivery Requsts at Index 
                var deliveryRequests = db.DeliveryRequests.Where(d => d.DeliveryManID == id && d.StatusID == 2).ToList();
      
                return View(deliveryRequests);
            }
            else if (Session["id"] != null)
            {
                int ID = Convert.ToInt32(Session["id"]);
                Employee employee = db.Employees.Find(ID);

                //List all Delivery Requsts at Index 
                var deliveryRequests = db.DeliveryRequests.Where(d => d.DeliveryManID == ID && d.StatusID == 2).ToList();

                return View(deliveryRequests);
            }


            return RedirectToAction("Index", "home");
        }

        ////////////////////////////////////

        [HttpPost]
        public bool updateLocation(Location loc)
        {
            Debug.WriteLine(" location => " + loc.location);
            if (loc != null)
            {
                int result = db.Database.ExecuteSqlCommand("UPDATE [Employee] set CurrentLocation = @location where ID = @dm_id", new SqlParameter("@location", loc.location), new SqlParameter("@dm_id", (int)Session["id"]));
                Debug.WriteLine(DateTime.Now.ToString("h:mm:ss tt") + " =>" + loc.location + " => " + Session["id"] + " => " + result);
            }

            return loc != null;
        }
        //_______________________________________________________________________________________ start of chattinf working
        public ActionResult Chat(int? id,int? usertype)//CustomerID or SuperVisorID &Type
        {
            if (Session["id"] != null)
            {
                if (usertype == 4 || usertype == 5)
                {
                    var Customer = db.Customers.Find(id);
                    if (Customer == null)
                    {
                        return HttpNotFound();
                    }

                    ViewBag.Receiver_UserName_onController = Customer.UserName;
                    ViewBag.Receiver_ID = id;
                    ViewBag.Customer_FullNAme = Customer.FirstName + " " + Customer.LastName;


                    int Deliveryman_ID = Convert.ToInt32(Session["id"]);
                    var messages = db.Messages.Where(msg => (msg.SenderID == Deliveryman_ID || msg.RecipientId == Deliveryman_ID) && (msg.SenderID == id || msg.RecipientId == id)).ToList();
                    if (messages != null)
                    {
                        return View(messages);

                    }
                    return View();
                }
                else if (usertype == 2)
                {
                    var SuperVisor = db.Employees.Find(id);
                    if (SuperVisor == null)
                    {
                        return HttpNotFound();
                    }

                    ViewBag.Receiver_UserName_onController = SuperVisor.UserName;
                    ViewBag.Receiver_ID = id;
                    ViewBag.Customer_FullNAme = SuperVisor.FirstName + " " + SuperVisor.LastName;


                    int Deliveryman_ID = Convert.ToInt32(Session["id"]);
                    var messages = db.Messages.Where(msg => (msg.SenderID == Deliveryman_ID || msg.RecipientId == Deliveryman_ID) && (msg.SenderID == id || msg.RecipientId == id)).ToList();
                    if (messages != null)
                    {
                        return View(messages);

                    }
                    return View();




                }
                else return HttpNotFound();
            }
            else return RedirectToAction("Index", "home");
            
        }

        //----------------- ConFirm Recieving --------------------\\
        [HttpPost]
        public ActionResult conFirmRecieving(int id,string code)
        {
            var req = db.DeliveryRequests.FirstOrDefault(o => o.ID == id);
            if (req == null)
            {
                return HttpNotFound();
            }
            else
            {
                
                try
                {
                    if(code == req.RecievingCode) { 
                    req.StatusID = 3; // '3' => recived
                    req.Paid = 2;
                    db.SaveChanges();
                    TempData["msg10"] = "<script>alert('Reciever Verified Successfully.');</script>";
                    return RedirectToAction("Index");
                    }else
                    {
                        TempData["msg10"] = "<script>alert('Confirmation code Incorrect! try again.');</script>";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error : "+ex.Message);
                    TempData["msg10"] = "<script>alert('Confirmation code Incorrect! try again.');</script>";
                    return RedirectToAction("Index");
                }
            }

        }

    }
    public class Location
    {
        public string location { get; set; }
    }

}