using SmartDelivery.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SmartDelivery.Controllers
{
    public class NormalCustomerController : Controller
    {
        SmartDeliveryEntities db = new SmartDeliveryEntities();

        // GET: NormalCustomer
        public ActionResult Index(int? id, int? usertype)
        {
            if (!HomeController.Authenticated && usertype != 4)
            {
                return RedirectToAction("Index", "home");
            }

            if (id != null)
            {
                Customer customer = db.Customers.Find(id);
                Session["userName"] = customer.UserName;
                Session["userType"] = customer.UserType.ToString();
                Session["id"] = id;
                //set cookie to can access from hubs
                Business.SetCookie(id.ToString(), customer.CustomerType.ToString());
                return RedirectToAction("ViewHome");
            }
            else if (Session["id"] != null)
            {
                int ID = Convert.ToInt32(Session["id"]);
                Customer customer = db.Customers.Find(id);
                return RedirectToAction("ViewHome");
            }


            return RedirectToAction("Index", "home");
        }

        /// <summary>
        /// Profile Managament //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// 

        [HttpGet]
        public ActionResult ViewProfile()
        {
            if(Session["id"] != null) { 
            int normalCustomerID = int.Parse(Session["id"].ToString());

            var normalCustomer = db.Customers.FirstOrDefault(n => n.ID == normalCustomerID);

            if (normalCustomer != null)
            {
                return View(normalCustomer);
            }
            else
            {
                return HttpNotFound();
            }
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult UpdateProfile(int? id)
        {
            if (Session["id"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Customer normalCustomer = db.Customers.FirstOrDefault(n => n.ID == id);
                if (normalCustomer == null)
                {
                    return HttpNotFound();
                }
                Session["photo"] = normalCustomer.Photo;
                Session["userName"] = normalCustomer.UserName;
                Session["oldPass"] = normalCustomer.PassWord;

                return View(normalCustomer);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult UpdateProfile(Customer customer , HttpPostedFileBase image)
        {
            if (image != null)
            {
                customer.Photo = new byte[image.ContentLength];
                image.InputStream.Read(customer.Photo, 0, image.ContentLength);
            }
            else
            {
                customer.Photo = (byte[])Session["photo"];
            }

            try
            {

                string oldUserName = Session["userName"].ToString();

                //Check if old username equal to the new or not
                if (customer.UserName != oldUserName)
                {
                    //check the username of the employee if exits or not 
                    Customer checkEmployeeExistInCustomers = new Customer();
                    checkEmployeeExistInCustomers = db.Customers.FirstOrDefault(emp => emp.UserName == customer.UserName);

                    Employee checkEmployeeExistInEmployees = new Employee();
                    checkEmployeeExistInEmployees = db.Employees.FirstOrDefault(emp => emp.UserName == customer.UserName);

                    if (checkEmployeeExistInCustomers != null || checkEmployeeExistInEmployees != null)
                    {
                        Response.Write("<script>alert('This UserName Is Not Available Type Another One !.');</script>");
                        return View(customer);
                    }
                }

                if(customer.PassWord != Session["oldPass"].ToString())
                {
                    customer.PassWord = Encryption.Encrypt(customer.PassWord);
                }
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                Response.Write("<script>alert('Customer Updated Successfully .');</script>");

                return RedirectToAction("ViewProfile");
            }
            catch
            {
                return View(customer);
            }
        }

        [HttpGet]
        public ActionResult DeleteAccount(int? id)
        {
            if (Session["id"] != null)
            {
                var customer = db.Customers.Find(id);
                if (customer == null)
                {
                    return HttpNotFound();
                }
                return View(customer);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult DeleteAccount(Customer _customer)
        {
            try
            {
                var customer = db.Customers.Find(_customer.ID);

                if (customer == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    // "0" Means that the customer deleted !
                    customer.Authorized = 0;
                    db.SaveChanges();
                    return RedirectToAction("LogOut", "Home");
                }
                
            }
            catch
            {
                return View(_customer);
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////




        /// <summary>
        /// Request Managament //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// list

        [HttpGet]
        public ActionResult ListDeliveryRequests()
        {
            if (Session["id"] != null)
            {
                Debug.WriteLine("----------------- " + HomeController.Authenticated);
                int customerId = int.Parse(Session["id"].ToString());

                var requests = db.DeliveryRequests.Where(request => request.CustomerID == customerId && request.StatusID != 4 && request.StatusID != 5).OrderByDescending(r => r.ID).ToList();

                return View(requests);
               
            }
            else {

                return RedirectToAction("Index", "home");
            }
                

        }

        [HttpGet]
        public ActionResult RequestDelivery()
        {
            if (Session["id"] != null)
            {
                //List of Shipment Types
                ViewBag.ShipmentTypeID = new SelectList(db.ShipmentTypes.ToList(), "ID", "Type");
                ViewBag.CostperKM = new SelectList(db.ShipmentTypes.ToList(), "ID", "PricePerKilo");
                return View();
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult RequestDelivery(DeliveryRequest deliveryRequest)
        {
            try
            {

                //List of Shipment Types
                
                //Debug.WriteLine("TOTAL COST : " + deliveryRequest.Cost);
                //Debug.WriteLine(" PAID : " + deliveryRequest.Paid);
                //Debug.WriteLine(" type : "+deliveryRequest.ShipmentTypeID);


                int customerID = int.Parse(Session["id"].ToString());

                deliveryRequest.CustomerID = customerID;
                deliveryRequest.StatusID = 1;  // "pending"
                

                if (Session["PaymentFlag"] != null)
                {
                    Debug.WriteLine(bool.Parse(Session["PaymentFlag"].ToString()) + " => PAID");
                    deliveryRequest.Paid = 2;
                    deliveryRequest.CurrentLocation = deliveryRequest.Source;
                    Session["PaymentFlag"] = null;
                }else { deliveryRequest.Paid = 1; }


                db.DeliveryRequests.Add(deliveryRequest);
                db.SaveChanges();
                return RedirectToAction("ListDeliveryRequests");

            }
            catch (Exception ex)
            {
                if(Session["PaymentFlag"] != null)
                Session["PaymentFlag"] = true;

                Debug.WriteLine("ERROR ! : " + ex.Message);
                ViewBag.ShipmentTypeID = new SelectList(db.ShipmentTypes.ToList(), "ID", "Type");
                ViewBag.CostperKM = new SelectList(db.ShipmentTypes.ToList(), "ID", "PricePerKilo");
                return View(deliveryRequest);
            }
        }


        [HttpGet]
        public ActionResult UpdateRequest(int? id)
        {
            if (Session["id"] != null)
            {
                //List of Shipment Types
                ViewBag.ShipmentType = new SelectList(db.ShipmentTypes.ToList(), "ID", "Type");
                ViewBag.CostperKM = new SelectList(db.ShipmentTypes.ToList(), "ID", "PricePerKilo");

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                DeliveryRequest deliveryRequest = db.DeliveryRequests.FirstOrDefault(d => d.ID == id);
                if (deliveryRequest == null)
                {
                    return HttpNotFound();
                }

                return View(deliveryRequest);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult UpdateRequest(DeliveryRequest deliveryRequest)
        {

            //List of Shipment Types
            ViewBag.ShipmentType = new SelectList(db.ShipmentTypes.ToList(), "ID", "Type");
            ViewBag.CostperKM = new SelectList(db.ShipmentTypes.ToList(), "ID", "PricePerKilo");

            if (Session["PaymentFlag"] != null)
            {
                Debug.WriteLine(bool.Parse(Session["PaymentFlag"].ToString()) + " => PAID");
                Session["PaymentFlag"] = null;
            }



            try
            {
                db.Entry(deliveryRequest).State = EntityState.Modified;
                db.SaveChanges();
                Response.Write("<script>alert('Delivery Request Updated Successfully .');</script>");

                return RedirectToAction("ListDeliveryRequests");
            }
            catch (Exception ex)
            {
                if (Session["PaymentFlag"] != null)
                    Session["PaymentFlag"] = true;

                Debug.WriteLine("ERROR ! : " + ex.Message);
                return View(deliveryRequest);
            }

        }

        [HttpGet]
        public ActionResult CancelRequest(int? id)   
        {
            if (Session["id"] != null)
            {
                var request = db.DeliveryRequests.Find(id);
                if (request == null)
                {
                    return HttpNotFound();
                }
                return View(request);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult CancelRequest(DeliveryRequest deliveryRequest)
        {

            var updatedDeliveryRequest = db.DeliveryRequests.FirstOrDefault(d => d.ID == deliveryRequest.ID);
            if (updatedDeliveryRequest == null)
            {
                return HttpNotFound();
            }
            else
            {
                try
                {
                    updatedDeliveryRequest.StatusID = 4; // "4" means the request canceled
                    db.SaveChanges();

                    return RedirectToAction("ListDeliveryRequests");
                }
                catch
                {
                    return View(updatedDeliveryRequest);
                }
            }
          

        }


        /////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Home //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// 

        [HttpGet]
        public ActionResult ViewHome()  //List all offers
        {
            if (Session["id"] != null)
            {
                var Offers = db.Offers.OrderByDescending(o => o.ID).ToList();
                return View(Offers);
            }
            else return RedirectToAction("Index", "home");
        }

        /////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Order Management //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// 

        [HttpGet]
        public ActionResult OrderOffer(int? id)
        {
            if (Session["id"] != null)
            {
                Session["Offer"] = db.Offers.FirstOrDefault(o => o.ID == id);
                return View();
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult OrderOffer(Order order)
        {
            try
            {

                int customerId = int.Parse(Session["id"].ToString());

                Offer offer = (Offer)Session["Offer"];


                order.CustomerID = customerId;
                order.OfferId = offer.ID;
                order.TotalPrice = order.Quantity * offer.Price;  // calculate the total price
                order.StatusId = 1;  //"1" pending order

                db.Orders.Add(order);
                db.SaveChanges();

                return RedirectToAction("ViewHome");

            }

            catch (Exception e)
            {
                ViewBag.e = e.InnerException;
                return View(order);
            }
        }

        [HttpGet]
        public ActionResult ListAllPurchases()
        {
            if (Session["id"] != null)
            {
                int customerId = int.Parse(Session["id"].ToString());
                var allOrders = db.Orders.Where(o => o.CustomerID == customerId && o.StatusId != 4).OrderByDescending(o => o.ID).ToList();
                return View(allOrders);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult EditOrder(int? id)
        {
            if (Session["id"] != null)
            {
                Order order = db.Orders.FirstOrDefault(o => o.ID == id);
                Session["Order"] = order;

                if (order == null)
                {
                    return HttpNotFound();
                }

                return View(order);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult EditOrder(Order order)
        {
            try
            {
                //old Order Data
                Order oldOrder = (Order)Session["Order"];


                //Ckeck if quantity Changed or not 
                if (order.Quantity != oldOrder.Quantity)
                {
                    //Update the quantity and the total price
                    Order updatedOrder = db.Orders.FirstOrDefault(o => o.ID == oldOrder.ID);

                    updatedOrder.Quantity = order.Quantity;
                    updatedOrder.TotalPrice = order.Quantity * oldOrder.Offer.Price;
                    updatedOrder.StatusId = 1;
                    db.SaveChanges();

                    return RedirectToAction("ListAllPurchases");
                }

                return RedirectToAction("ListAllPurchases");
            }
            catch
            {
                return View(order);
            }
        }

        [HttpGet]
        public ActionResult CancelOrder(int? id)
        {
            if (Session["id"] != null)
            {
                var order = db.Orders.Find(id);
                if (order == null)
                {
                    return HttpNotFound();
                }
                return View(order);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult CancelOrder(Order order)
        {

            var updatedOrder = db.Orders.FirstOrDefault(d => d.ID == order.ID);
            if (updatedOrder == null)
            {
                return HttpNotFound();
            }
            else
            {
                try
                {
                    updatedOrder.StatusId = 4; // "4" means the order canceled
                    db.SaveChanges();

                    return RedirectToAction("ListAllPurchases");
                }
                catch
                {
                    return View(order);
                }
            }


        }
        //////////-----------////////////////

        [HttpPost]
        public ActionResult removeRequest(int? id)
        {

            var Request = db.DeliveryRequests.FirstOrDefault(d => d.ID == id);
            if (Request == null)
            {
                return HttpNotFound();
            }
            else
            {
                try
                {
                    Request.StatusID = 5; // "5" means the order removed
                    db.SaveChanges();

                    return RedirectToAction("ListDeliveryRequests");
                }
                catch
                {
                    TempData["msg10"] = "<script>alert('Error !');</script>";
                    return RedirectToAction("ListDeliveryRequests");
                }
            }


        }
        /////////////////////////////////////////////////////////////////////////////////////





        /// <summary>
        /// Maps Functions of DeliveryRequest "Ramadan's Functions" //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// 

        public ActionResult getCurrentLocation(DeliveryRequestID RID)
        {

            if (RID != null)
            {
                var result = new CurrentLocation { c_location = "" };
                result.c_location = db.DeliveryRequests.FirstOrDefault(d => d.ID == RID.id).CurrentLocation;


                Debug.WriteLine(DateTime.Now.ToString("h:mm:ss tt") + " =>" + RID.id + " => " + result.c_location + " =");

                return Json(new { employee = result }, JsonRequestBehavior.AllowGet);
            }





            return Json(new { employee = new CurrentLocation { c_location = "Location Not Found!" } }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getjourneyInfo(DeliveryRequestID RID)
        {

            if (RID != null)
            {
                var result = new journey { source = "", destination = "", current_loc = "" };
                var request = db.DeliveryRequests.FirstOrDefault(d => d.ID == RID.id);
                result.source = request.Source;
                result.destination = request.Destination;
                result.current_loc = request.CurrentLocation;
                Debug.WriteLine(DateTime.Now.ToString("h:mm:ss tt") + " =>" + RID.id + " => " + result.source + " = " + result.destination);

                return Json(new { journy = result }, JsonRequestBehavior.AllowGet);
            }





            return Json(new { employee = new CurrentLocation { c_location = "Location Not Found!" } }, JsonRequestBehavior.AllowGet);
        }
        //________________ORDER PAYMENT__________________\\

        [HttpPost]
        public bool payOrder(OrderID Ord_ID)
        {
            Debug.WriteLine(" Ord_ID => " + Ord_ID.id);
            if (Ord_ID != null)
            {
                int result = db.Database.ExecuteSqlCommand("Update  [dbo].[Order] SET Paid = 2 where ID = @orderid", new SqlParameter("@orderid", Ord_ID.id));
                Debug.WriteLine("Result => " + result);
                Session["OrderPaymentFlag"] = null;
            }

            return Ord_ID != null;
        }

        [HttpPost]
        public bool payDeliveryRequest(DeliveryRequestID Ord_ID)
        {
           // Debug.WriteLine(" Ord_ID => " + Ord_ID.id);
            if (Ord_ID != null)
            {
                var req = db.DeliveryRequests.Find(Ord_ID.id);
                req.Paid = 2;
                db.SaveChanges();
                Session["PaymentFlag"] = null;
            }

            return Ord_ID != null;
        }
        //___________________________________________________________________________________________________________________statr of chatting working
        //public ActionResult Chat(int? id)//DeliveryManId
        //{
        //    var DeliveryMan = db.Employees.Find(id);
        //    if (DeliveryMan == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    else
        //    {
        //        ViewBag.Receiver_UserName_onController = DeliveryMan.UserName;
        //        ViewBag.Receiver_ID = id;
        //        ViewBag.DeliveryMan_FullNAme = DeliveryMan.FirstName + " " + DeliveryMan.LastName;
        //    }
        //    int customerId = Convert.ToInt32(Session["id"]);
        //    //var requests = db.DeliveryRequests.Where(request => request.CustomerID == customerId && request.StatusID == 2).ToList();
        //    var messages = db.Messages.Where(msg => (msg.SenderID == customerId || msg.RecipientId == customerId) && (msg.SenderID == id || msg.RecipientId == id)).ToList();

        //    if (messages != null)
        //    {
        //        return View(messages);
        //    }
        //    else { return View(); }
        //}

        //_______________________________________________________________________________________ start of chattinf working
        public ActionResult Chat(int? id, int? usertype)//Delivery man or SuperVisorID &Type
        {
            if (Session["id"] != null)
            {
                if (usertype == 3)
                {
                    var DeliveryMan = db.Employees.Find(id);
                    if (DeliveryMan == null)
                    {
                        return HttpNotFound();
                    }

                    ViewBag.Receiver_UserName_onController = DeliveryMan.UserName;
                    ViewBag.Receiver_ID = id;
                    ViewBag.DeliveryMan_FullNAme = DeliveryMan.FirstName + " " + DeliveryMan.LastName;


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
                    ViewBag.DeliveryMan_FullNAme = SuperVisor.FirstName + " " + SuperVisor.LastName;
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

        [HttpPost]
        public bool rateRequest(rate rateObj)
        {
           // Debug.WriteLine(" Ord_ID => " + Ord_ID.id);
            if (rateObj != null)
            {
                var req = db.DeliveryRequests.Find(rateObj.reqID);
                req.Rated = rateObj.empRate;
                var rat = db.Rates.FirstOrDefault(Ob => Ob.EmpID == rateObj.empID);
                if(rat != null)
                {
                    rat.RequestsNumber = rat.RequestsNumber + 1;
                    rat.EmpRate = rat.EmpRate + (rateObj.empRate/5);
                    
                }
                else
                {
                    var newrate = new Rate();
                    newrate.EmpID = rateObj.empID;
                    newrate.RequestsNumber = 1;
                    newrate.EmpRate = rateObj.empRate / 5;
                    db.Rates.Add(newrate);
                }
                db.SaveChanges();
            }

            return rateObj != null;
        }
    }
    public class OrderID
    {
        public int id
        {
            get; set;
        }
    }

    public class DeliveryRequestID
    {
        public int id { get; set; }
    }
    public class CurrentLocation
    {
        public string c_location { get; set; }
    }
    public class journey
    {
        public string source { get; set; }
        public string destination { get; set; }
        public string current_loc { get; set; }
    }

    public class rate
    {
        public int reqID { get; set; }
        public int empID { get; set; }
        public int empRate { get; set; }
    }


}