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
    public class ShopController : Controller
    {
        SmartDeliveryEntities db = new SmartDeliveryEntities();

        // GET: Shop
        public ActionResult Index(int? id, int? usertype)
        {
            if (!HomeController.Authenticated && usertype != 5)
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
                return View(customer);
            }
            else if (Session["id"] != null)
            {
                int ID = Convert.ToInt32(Session["id"]);
                Customer customer = db.Customers.Find(id);
                return View(customer);
            }


            return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult ViewProfile()
        {
            if(Session["id"] != null)
            {
                int shopID = int.Parse(Session["id"].ToString());

                var shop = db.Customers.FirstOrDefault(n => n.ID == shopID);

                if (shop != null)
                {
                    return View(shop);
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

                Customer shop = db.Customers.FirstOrDefault(s => s.ID == id);
                if (shop == null)
                {
                    return HttpNotFound();
                }
                Session["photo"] = shop.Photo;
                Session["userName"] = shop.UserName;
                Session["oldPass"] = shop.PassWord;
                return View(shop);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult UpdateProfile(Customer shop, HttpPostedFileBase image)
        {
            if (image != null)
            {
                shop.Photo = new byte[image.ContentLength];
                image.InputStream.Read(shop.Photo, 0, image.ContentLength);
            }
            else
            {
                shop.Photo = (byte[])Session["photo"];
            }

            try
            {

                string oldUserName = Session["userName"].ToString();

                //Check if old username equal to the new or not
                if (shop.UserName != oldUserName)
                {
                    //check the username of the employee if exits or not 
                    Customer checkEmployeeExistInCustomers = new Customer();
                    checkEmployeeExistInCustomers = db.Customers.FirstOrDefault(emp => emp.UserName == shop.UserName);

                    Employee checkEmployeeExistInEmployees = new Employee();
                    checkEmployeeExistInEmployees = db.Employees.FirstOrDefault(emp => emp.UserName == shop.UserName);

                    if (checkEmployeeExistInCustomers != null || checkEmployeeExistInEmployees != null)
                    {
                        Response.Write("<script>alert('This UserName Is Not Available Type Another One !.');</script>");
                        return View(shop);
                    }
                }
                if(shop.PassWord != Session["oldPass"].ToString())
                {
                    shop.PassWord = Encryption.Encrypt(shop.PassWord);
                }

                db.Entry(shop).State = EntityState.Modified;
                db.SaveChanges();
                Response.Write("<script>alert('Customer Updated Successfully .');</script>");

                return RedirectToAction("ViewProfile");
            }
            catch
            {
                return View(shop);
            }
        }

        [HttpGet]
        public ActionResult DeleteAccount(int? id)
        {
            if (Session["id"] != null)
            {
                var shop = db.Customers.Find(id);
                if (shop == null)
                {
                    return HttpNotFound();
                }
                return View(shop);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult DeleteAccount(Customer _shop)
        {
            try
            {
                var shop = db.Customers.Find(_shop.ID);

                if (shop == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    // "0" Means that the shop deleted !
                    shop.Authorized = 0;
                    db.SaveChanges();
                    return RedirectToAction("LogOut", "Home");
                }

            }
            catch
            {
                return View(_shop);
            }
        }


        /// <summary>
        /// Offers Managament //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// 

        public ActionResult ListAllOffers()
        {
            if (Session["id"] != null)
            {
                int shopId = int.Parse(Session["id"].ToString()); //shop Id

                var offers = db.Offers.Where(o => o.ShopID == shopId).ToList();
                return View(offers);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult AddOffer()
        {
            if (Session["id"] != null)
            {
                return View();
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult AddOffer(Offer offer , HttpPostedFileBase images)
        {
            try
            {

                    int shopId = int.Parse(Session["id"].ToString()); //shop Id
                int result = DateTime.Compare(offer.StartDate, offer.EndDate);
             
                if (result > 0)
                {
                    Response.Write("<script>alert('Start Date couldnt be later than End date.');</script>");
                    return View(offer);
                }

                if (images != null)
                    {
                           
                            
                                offer.ShopID = shopId;
                                db.Offers.Add(offer);
                                db.SaveChanges();

                                int offerId = offer.ID;  //Offer Id

                                
                                    OfferImage photo = new OfferImage();

                                    photo.Image = new byte[images.ContentLength];
                                    images.InputStream.Read(photo.Image, 0, images.ContentLength);

                                    photo.OfferID = offerId;

                                    db.OfferImages.Add(photo);
                                    db.SaveChanges();
                                    
                                

                                return RedirectToAction("ListAllOffers");
                           }
                    
                    else
                    {
                        //Check image is submitted or not 
                        
                            Response.Write("<script>alert('Select Photo Please.');</script>");
                        
                    }   

                return View(offer);
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('Exception');</script>");
                Response.Write("<script>alert('"+ex.Message+"');</script>");
               
                return View(offer);
            }
        }

        [HttpGet]
        public ActionResult OfferDetails(int? id)
        {
            if (Session["id"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Offer offer = db.Offers.FirstOrDefault(o => o.ID == id);

                if (offer == null)
                {
                    return HttpNotFound();
                }

                return View(offer);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult EditOffer(int? id)
        {
            if (Session["id"] != null)
            {
                var offer = db.Offers.Find(id);
                //ده حل لإرور كان مجنني بسبب W3C Specifications
                ViewBag.StartDate = offer.StartDate.ToString("yyyy-MM-ddTHH:mmK");
                ViewBag.EndDate = offer.EndDate.ToString("yyyy-MM-ddTHH:mmK");

                if (offer == null)
                {
                    return HttpNotFound();
                }
                return View(offer);
            }
            else return RedirectToAction("Index", "home");
        }
        [HttpPost]
        public ActionResult EditOffer(Offer offer, HttpPostedFileBase images)
        {
            try
            {
                int result = DateTime.Compare(offer.StartDate, offer.EndDate);

                if (result > 0)
                {
                    Response.Write("<script>alert('Start Date couldnt be later than End date.');</script>");
                    return View(offer);
                }

                //Update the offer
                db.Entry(offer).State = EntityState.Modified;
                    db.SaveChanges();

                    //update the images of the offer
                    if (images != null)
                    {
                        
                        
                        
                            int offerId = offer.ID;  //Offer Id

                            //Delete old Images
                            db.OfferImages.RemoveRange(db.OfferImages.Where(i => i.OfferID == offerId));

                            
                                OfferImage photo = new OfferImage();

                                photo.Image = new byte[images.ContentLength];
                                images.InputStream.Read(photo.Image, 0, images.ContentLength);

                                photo.OfferID = offerId;

                                db.OfferImages.Add(photo);
                                db.SaveChanges();
                            

                        

                    }// End IF
                

                return RedirectToAction("ListAllOffers");

            }
            catch
            {
                ViewBag.StartDate = offer.StartDate.ToString("yyyy-MM-ddTHH:mmK");
                ViewBag.EndDate = offer.EndDate.ToString("yyyy-MM-ddTHH:mmK");
                return View(offer);
            }
        }

        [HttpGet]
        public ActionResult DeleteOffer(int? id)
        {
            if (Session["id"] != null)
            {
                var offer = db.Offers.Find(id);
                ViewBag.OfferImages = db.OfferImages.Where(o => o.OfferID == id).ToList();
                if (offer == null)
                {
                    return HttpNotFound();
                }
                return View(offer);
            }
            else return RedirectToAction("Index", "home");

        }

        [HttpPost]
        public ActionResult DeleteOffer(Offer _offer)
        {
            try
            {
                //First Delete  Images
                db.OfferImages.RemoveRange(db.OfferImages.Where(i => i.OfferID == _offer.ID));

                //then Remove the Offer
                var myoffer = db.Offers.Find(_offer.ID);
                db.Offers.Remove(myoffer);

                db.SaveChanges();

                return RedirectToAction("ListAllOffers");
            }
            catch
            {
                return View(_offer);
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////



        /// <summary>
        /// Order Managament //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// 

        [HttpGet]
        public ActionResult ListAllOrders()
        {
            //ShopID
            if (Session["id"] != null)
            {
                int shopId = int.Parse(Session["id"].ToString());

                //All pending Order
                var orders = db.Orders.Where(o => o.Offer.ShopID == shopId && o.StatusId == 1).ToList();

                return View(orders);
            }
            else return RedirectToAction("Index", "home");
            //return HttpNotFound(); كانت مكتوبة كده 
        }


        [HttpPost]
        public ActionResult AcceptOrder(int id)
        {
            var order = db.Orders.FirstOrDefault(o => o.ID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            else
            {
                try
                {
                    order.StatusId = 2; // '2' => Accepted
                    db.SaveChanges();
                    return RedirectToAction("ListAllOrders");
                }
                catch
                {
                    return View(order);
                }
            }

        }

        [HttpPost]
        public bool AcceptAjaxOrder(OrderID Ord_ID)
        {
            Debug.WriteLine(" Ord_ID => " + Ord_ID.id);
            if (Ord_ID != null)
            {
                int result = db.Database.ExecuteSqlCommand("Update  [dbo].[Order] SET StatusId = 2 where ID = @orderid", new SqlParameter("@orderid", Ord_ID.id));
                Debug.WriteLine("Result => " + result);
            }

            return Ord_ID != null;
        }

        [HttpPost]
        public ActionResult RefuseOrder(int? id)
        {
            var order = db.Orders.FirstOrDefault(o => o.ID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            else
            {
                try
                {
                    order.StatusId = 3; // '3' => Refused
                    db.SaveChanges();
                    return RedirectToAction("ListAllOrders");
                }
                catch
                {
                    return View(order);
                }
            }
        }
        /// <summary>
        /// Delivery Management
        /// </summary>
        /// <param name="_deliveryRequest"></param>
        /// <returns></returns>
        //____________________make Delivery Request_________________________\\
        [HttpPost]
        public bool RequestDelivery(ShopDeliveryRequest _deliveryRequest)
        {
            if(_deliveryRequest != null) { 
            try
            {
                    //Debug.WriteLine("TOTAL COST : " + deliveryRequest.Cost);
                    //Debug.WriteLine(" PAID : " + deliveryRequest.Paid);
                    //Debug.WriteLine(" type : "+deliveryRequest.ShipmentTypeID);

                var deliveryRequest = new DeliveryRequest();
                int customerID = int.Parse(Session["id"].ToString());

                deliveryRequest.CustomerID = customerID;
                deliveryRequest.StatusID = 1;  // "pending"
                deliveryRequest.Paid = 2;
                deliveryRequest.ShipmentDescription = _deliveryRequest.ShipmentDescription;
                deliveryRequest.Source = _deliveryRequest.Source;
                deliveryRequest.Destination = _deliveryRequest.Destination;
                deliveryRequest.RecievingCode = "2018";
                deliveryRequest.ShipmentWeight = 0;



                    db.DeliveryRequests.Add(deliveryRequest);
                db.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {

                    Response.Write("<script>alert('Error!"+ ex.Message + "');</script>");
                    Debug.WriteLine("ERROR ! : " + ex.Message);
                return false;
            }
            }else return false;
        }
        /////////////////////////////// Request Custom Delivery \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
        [HttpGet]
        public ActionResult RequestCustomDelivery()
        {
            if (Session["id"] != null)
            {
                ViewBag.ShipmentTypeID = new SelectList(db.ShipmentTypes.ToList(), "ID", "Type");
                return View();
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult RequestCustomDelivery(DeliveryRequest deliveryRequest)
        {
            try
            {
                

                int customerID = int.Parse(Session["id"].ToString());

                deliveryRequest.CustomerID = customerID;
                deliveryRequest.StatusID = 1;  // "pending"
                deliveryRequest.Paid = 2;
                deliveryRequest.ShipmentWeight = 0;
                deliveryRequest.RecievingCode = "2018";

                db.DeliveryRequests.Add(deliveryRequest);
                db.SaveChanges();
                return RedirectToAction("ListDeliveryRequests");

            }
            catch (Exception ex)
            {
                ViewBag.ShipmentTypeID = new SelectList(db.ShipmentTypes.ToList(), "ID", "Type");
                Response.Write("<script>alert('"+ex.Message+"');</script>");
                return View(deliveryRequest);
            }
        }
        [HttpGet]
        public ActionResult ListDeliveryRequests()
        {
            if (Session["id"] != null)
            {
                int customerId = int.Parse(Session["id"].ToString());

                var requests = db.DeliveryRequests.Where(request => request.CustomerID == customerId && request.StatusID != 5).OrderByDescending(r => r.ID).ToList();

                return View(requests);
            }
            else
                return RedirectToAction("Index", "home");

        }
        [HttpGet]
        public ActionResult UpdateRequest(int? id)
        {
            if (Session["id"] != null)
            {
                if (id == null)
                {
                    return RedirectToAction("Index", "home");
                }

                DeliveryRequest deliveryRequest = db.DeliveryRequests.FirstOrDefault(d => d.ID == id);
                if (deliveryRequest == null)
                {
                    return RedirectToAction("Index", "home");
                }

                return View(deliveryRequest);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult UpdateRequest(DeliveryRequest deliveryRequest)
        {

        


            try
            {
                
                deliveryRequest.StatusID = 1;  // "pending"
               
                
          
                deliveryRequest.RecievingCode = "2018";
                deliveryRequest.ShipmentWeight = 0;
                Debug.WriteLine(deliveryRequest.CustomerID + " - "+deliveryRequest.ShipmentTypeID + " - "+deliveryRequest.ShipmentDescription+" - "+ deliveryRequest.Paid);
                db.Entry(deliveryRequest).State = EntityState.Modified;
                db.SaveChanges();
                Response.Write("<script>alert('Delivery Request Updated Successfully .');</script>");

                return RedirectToAction("ListDeliveryRequests");
            }
            catch (Exception ex)
            {

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

        //__________________END DELIVERY Request_______________________\\
        //_______________________________________________________________________________________ start of chatting wprking

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


    }

    public class ShopDeliveryRequest
    {
        public string ShipmentDescription { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
    }

    
}