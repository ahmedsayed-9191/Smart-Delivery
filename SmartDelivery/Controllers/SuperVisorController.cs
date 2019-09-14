using SmartDelivery.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SmartDelivery.Controllers
{
    public class SuperVisorController : Controller
    {
        SmartDeliveryEntities db = new SmartDeliveryEntities();

        // GET: SuperVisor
        public ActionResult Index(int? id, int? usertype)
        {
            if (!HomeController.Authenticated && usertype != 2)
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
                return View(employee);
            }
            else if (Session["id"] != null)
            {
                int ID = Convert.ToInt32(Session["id"]);
                Employee employee = db.Employees.Find(id);
                return View(employee);
            }


            return RedirectToAction("Index", "home");
        }

        /// <summary>
        /// DeliveryMan Managament //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// 

        [HttpGet]
        public ActionResult ListAllDeliveryMan()
        {
            if (Session["id"] != null)
            {
                //List all employess with usertype "3"
                //without refused with tag "3"
                var deliveryman = db.Employees.Where(s => s.EmployeeType == 3 && s.Authorized != 3 && s.Authorized != -1).ToList();
                return View(deliveryman);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult ListAllRefusedDeliveryMan()
        {
            if (Session["id"] != null)
            {
                //List all employess with usertype "3"
                //refused with tag "3"
                var deliveryman = db.Employees.Where(s => s.EmployeeType == 3 && s.Authorized == 3).ToList();
                return View(deliveryman);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult AddDeliveryMan()
        {
            if (Session["id"] != null)
            {
                //List Of Days 
                ViewBag.HolidayID = new SelectList(db.Days.ToList(), "ID", "DayName");
                //List Of Shifts
                ViewBag.ShiftID = new SelectList(db.Shifts.ToList(), "ID", "StartTime");
                return View();
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult AddDeliveryMan(Employee employee, HttpPostedFileBase image)
        {

            //List Of Days 
            ViewBag.HolidayID = new SelectList(db.Days.ToList(), "ID", "DayName");
            //List Of Shifts
            ViewBag.ShiftID = new SelectList(db.Shifts.ToList(), "ID", "StartTime");

            if (image != null)
            {
                //Save image 
                employee.Photo = new byte[image.ContentLength];
                image.InputStream.Read(employee.Photo, 0, image.ContentLength);

                //check the username of the employee if exits or not 
                Customer checkEmployeeExistInCustomers = new Customer();
                checkEmployeeExistInCustomers = db.Customers.FirstOrDefault(emp => emp.UserName == employee.UserName);

                Employee checkEmployeeExistInEmployees = new Employee();
                checkEmployeeExistInEmployees = db.Employees.FirstOrDefault(emp => emp.UserName == employee.UserName);

                if (checkEmployeeExistInCustomers != null || checkEmployeeExistInEmployees != null)
                {
                    Response.Write("<script>alert('This UserName Is Not Available Type Another One !.');</script>");
                    return View(employee);
                }

                //add employee to database
                Employee myEmployee = new Employee();
                myEmployee.FirstName = employee.FirstName;
                myEmployee.LastName = employee.LastName;
                myEmployee.Email = employee.Email;
                myEmployee.Phone = employee.Phone;
                myEmployee.Address = employee.Address;
                myEmployee.Photo = employee.Photo;
                myEmployee.UserName = employee.UserName;
                myEmployee.PassWord = Encryption.Encrypt(employee.PassWord);
                myEmployee.Salary = employee.Salary;
                myEmployee.Authorized = 2; // => "2" means need to approval
                myEmployee.Active = 0; // means he is Offfline
                myEmployee.EmployeeType = 3; // => "3" the type of the deliveryman
                myEmployee.ShiftID = employee.ShiftID;
                myEmployee.HolidayID = employee.HolidayID;

                db.Employees.Add(myEmployee);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('invalid Data! Try again.');</script>");
                    return View(employee);

                }

                Response.Write("<script>alert('" + employee.FirstName + " " + employee.LastName + " Added Successfully.');</script>");
                TempData["msg10"] = "<script>alert('" + employee.FirstName + " " + employee.LastName + " Added Successfully.');</script>";


                return RedirectToAction("ListAllDeliveryMan");
            }
            else
            {
                //Check image is submitted or not 
                if (image == null)
                {
                    Response.Write("<script>alert('Select Photo Please.');</script>");
                }

                return View(employee);

            }
        }

        [HttpGet]
        public ActionResult DetailsOfDeliveryMan(int? id)
        {
            if (Session["id"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Employee deliveryMan = db.Employees.FirstOrDefault(D => D.ID == id);

                if (deliveryMan == null)
                {
                    return HttpNotFound();
                }

                return View(deliveryMan);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult UpdateDeliveryMan(int? id)
        {
            if (Session["id"] != null)
            {
                //List Of Days 
                ViewBag.HolidayID = new SelectList(db.Days.ToList(), "ID", "DayName");
                //List Of Shifts
                ViewBag.ShiftID = new SelectList(db.Shifts.ToList(), "ID", "StartTime");


                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Employee deliveryMan = db.Employees.FirstOrDefault(D => D.ID == id);
                if (deliveryMan == null)
                {
                    return HttpNotFound();
                }
                Session["photo"] = deliveryMan.Photo;
                Session["userName"] = deliveryMan.UserName;
                Session["oldPass"] = deliveryMan.PassWord;

                return View(deliveryMan);
            }
            else return RedirectToAction("Index", "home");
        }   

        [HttpPost]
        public ActionResult UpdateDeliveryMan(Employee deliveryMan, HttpPostedFileBase image)
        {
            //List Of Days 
            ViewBag.HolidayID = new SelectList(db.Days.ToList(), "ID", "DayName");
            //List Of Shifts
            ViewBag.ShiftID = new SelectList(db.Shifts.ToList(), "ID", "StartTime");

            if (image != null)
            {
                deliveryMan.Photo = new byte[image.ContentLength];
                image.InputStream.Read(deliveryMan.Photo, 0, image.ContentLength);
            }
            else
            {
                deliveryMan.Photo = (byte[])Session["photo"];
            }

            try
            {

                string oldUserName = Session["userName"].ToString();

                //Check if old username equal to the new or not
                if (deliveryMan.UserName != oldUserName)
                {
                    //check the username of the employee if exits or not 
                    Customer checkEmployeeExistInCustomers = new Customer();
                    checkEmployeeExistInCustomers = db.Customers.FirstOrDefault(emp => emp.UserName == deliveryMan.UserName);

                    Employee checkEmployeeExistInEmployees = new Employee();
                    checkEmployeeExistInEmployees = db.Employees.FirstOrDefault(emp => emp.UserName == deliveryMan.UserName);

                    if (checkEmployeeExistInCustomers != null || checkEmployeeExistInEmployees != null)
                    {
                        Response.Write("<script>alert('This UserName Is Not Available Type Another One !.');</script>");
                        return View(deliveryMan);
                    }
                }
                if (deliveryMan.PassWord != Session["oldPass"].ToString())
                {
                    deliveryMan.PassWord = Encryption.Encrypt(deliveryMan.PassWord);
                }
                db.Entry(deliveryMan).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('invalid Data! Try again.');</script>");
                    return View(deliveryMan); 

                }
                Response.Write("<script>alert('Supervisor Updated Successfully .');</script>");

                return RedirectToAction("ListAllDeliveryMan");
            }
            catch
            {
                return View(deliveryMan);
            }

        }

        [HttpGet]
        public ActionResult ReCreateDeliveryMan(int? id)
        {
            if (Session["id"] != null)
            {
                //List Of Days 
                ViewBag.HolidayID = new SelectList(db.Days.ToList(), "ID", "DayName");
                //List Of Shifts
                ViewBag.ShiftID = new SelectList(db.Shifts.ToList(), "ID", "StartTime");


                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Employee deliveryMan = db.Employees.FirstOrDefault(D => D.ID == id);
                if (deliveryMan == null)
                {
                    return HttpNotFound();
                }

                Session["oldDeliveryManId"] = deliveryMan.ID;

                return View(deliveryMan);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult ReCreateDeliveryMan(Employee employee, HttpPostedFileBase image)
        {
           
            //List Of Days 
            ViewBag.HolidayID = new SelectList(db.Days.ToList(), "ID", "DayName");
            //List Of Shifts
            ViewBag.ShiftID = new SelectList(db.Shifts.ToList(), "ID", "StartTime");

            if (image != null)
            {

                //First delete old data of the deliveryMan
                int oldDeliveryManID = int.Parse(Session["oldDeliveryManId"].ToString());
                var oldDeliveryMan = db.Employees.FirstOrDefault(d => d.ID == oldDeliveryManID);
                if (oldDeliveryMan != null)
                {

                    db.Employees.Remove(oldDeliveryMan);
                    db.SaveChanges();
                }

                //Save image 
                employee.Photo = new byte[image.ContentLength];
                image.InputStream.Read(employee.Photo, 0, image.ContentLength);

                //check the username of the employee if exits or not 
                Customer checkEmployeeExistInCustomers = new Customer();
                checkEmployeeExistInCustomers = db.Customers.FirstOrDefault(emp => emp.UserName == employee.UserName);

                Employee checkEmployeeExistInEmployees = new Employee();
                checkEmployeeExistInEmployees = db.Employees.FirstOrDefault(emp => emp.UserName == employee.UserName);

                if (checkEmployeeExistInCustomers != null || checkEmployeeExistInEmployees != null)
                {
                    Response.Write("<script>alert('This UserName Is Not Available Type Another One !.');</script>");
                    return View(employee);
                }

                //add employee to database
                Employee myEmployee = new Employee();
                myEmployee.FirstName = employee.FirstName;
                myEmployee.LastName = employee.LastName;
                myEmployee.Email = employee.Email;
                myEmployee.Phone = employee.Phone;
                myEmployee.Address = employee.Address;
                myEmployee.Photo = employee.Photo;
                myEmployee.UserName = employee.UserName;
                myEmployee.PassWord = Encryption.Encrypt(employee.PassWord);
                myEmployee.Salary = employee.Salary;
                myEmployee.Authorized = 2; // => "2" means need to approval
                myEmployee.Active = 0; // means he is Offfline
                myEmployee.EmployeeType = 3; // => "3" the type of the deliveryman
                myEmployee.ShiftID = employee.ShiftID;
                myEmployee.HolidayID = employee.HolidayID;
                try
                {
                    db.Employees.Add(myEmployee);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('invalid Data! Try again.');</script>");
                    return View(employee);

                }


                Response.Write("<script>alert('" + employee.FirstName + " " + employee.LastName + " Added Successfully.');</script>");
                TempData["msg10"] = "<script>alert('" + employee.FirstName + " " + employee.LastName + " Added Successfully.');</script>";


                return RedirectToAction("ListAllRefusedDeliveryMan");
            }
            else
            {
                //Check image is submitted or not 
                if (image == null)
                {
                    Response.Write("<script>alert('Select Photo Please.');</script>");
                }

                return View(employee);

            }
        }

        [HttpGet]
        public ActionResult ComplaintToAdmin(int? id)
        {
            if (Session["id"] != null)
            {
                Session["deliveryManId"] = id;
                return View();
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult ComplaintToAdmin(Complaint complaint)
        {
            try
            {

                //get supervisor id from session
                int superVisorId = int.Parse(Session["id"].ToString());

                //get delivery man id from session
                int deliveryManId = int.Parse(Session["deliveryManId"].ToString());

                // complete the complaint data
                complaint.SuperVisorID = superVisorId;
                complaint.DeliveryManID = deliveryManId;

                db.Complaints.Add(complaint);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('invalid Data! Try again.');</script>");
                    return View(complaint);

                }

                Response.Write("<script>alert('Complaint Sent Successfully !.');</script>");

                return RedirectToAction("ListAllDeliveryMan");

            }
            catch
            {
                return View(complaint);
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////////



        /// <summary>
        /// Delivery Requests Managament //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// 

        [HttpGet]
        public ActionResult ListDeliveryRequests()
        {
            if (Session["id"] != null)
            {
                var requests = db.DeliveryRequests.Where(req => req.StatusID != 5).ToList();
                return View(requests);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult DetailsOfDeliveryRequest(int? id)
        {
            if (Session["id"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                DeliveryRequest deliveryRequest = db.DeliveryRequests.FirstOrDefault(D => D.ID == id);

                if (deliveryRequest == null)
                {
                    return HttpNotFound();
                }

                return View(deliveryRequest);
            }
            else return RedirectToAction("Index", "home");
        }


        [HttpGet]
        public ActionResult ListAvailableDeliveryMen()  //to assign one of them to delievry request
        {
            if (Session["id"] != null)
            {
                //Id of the Request that need
                // Session["RequestID"] = id;

                // var timeNow = DateTime.Now.ToShortTimeString();  //Need to alter the condition to detect available DeliveyMen
                var availableDeliveryMen = db.Employees
                .SqlQuery("select * from Employee where EmployeeType =3 and Employee.Authorized =1  and Employee.ID not in (select DeliveryRequest.DeliveryManID from DeliveryRequest where DeliveryRequest.StatusID = 2)")
                .Select(x => new DeliveryMan
                {
                    id = x.ID,
                    fullName = x.FirstName + " " + x.LastName,
                    currentLocation = x.CurrentLocation
                }).ToList();

                return Json(new { data = availableDeliveryMen }, JsonRequestBehavior.AllowGet);
            }
            else return RedirectToAction("Index", "home");
        }


        [HttpPost] 
        public bool AssignDeliveyManToRequest(Delivery del_obj)// Id of the deliveryMan
        {
            //Deleivey Request Id 
            if (del_obj != null)
            {
                int requestId = del_obj.requestID;

                var deliveryRequest = db.DeliveryRequests.FirstOrDefault(r => r.ID == requestId);

                if (deliveryRequest == null)
                {
                    Debug.WriteLine("REQUEST NO FOUND");
                    return false;
                }
                else
                {
                        deliveryRequest.SupervisorID = Convert.ToInt32(Session["id"]);       
                        deliveryRequest.DeliveryManID = del_obj.deliveryManID;
                        deliveryRequest.StatusID = 2;
                        deliveryRequest.CurrentLocation = del_obj.currentLocation;
                        db.SaveChanges();

                        return true;
                }
            }
            else return false;
        }

        [HttpPost]
        public bool removeRequest(DeliveryRequestID RID)
        {
          
            if (RID != null)
            {
                var request = db.DeliveryRequests.Find(RID.id);
                request.StatusID = 5; // removed
                db.SaveChanges();
            }

            return RID != null;
        }
        [HttpPost]
        public bool removeDeliveryMan(DeliveryMan DMID)
        {

            if (DMID != null)
            {
                var emp = db.Employees.Find(DMID.id);
                emp.Authorized = -1; // removed
                db.SaveChanges();
            }

            return DMID != null;
        }
        //_______________________________________________________________________________________ start of chatting work
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
        //    int SupervisorId = Convert.ToInt32(Session["id"]);
        //    //var requests = db.DeliveryRequests.Where(request => request.CustomerID == customerId && request.StatusID == 2).ToList();
        //    var messages = db.Messages.Where(msg => (msg.SenderID == SupervisorId || msg.RecipientId == SupervisorId) && (msg.SenderID == id || msg.RecipientId == id)).ToList();

        //    if (messages != null)
        //    {
        //        return View(messages);
        //    }
        //    else { return View(); }
        //}
        //_______________________________________________________________________________________ start of chattinf working
        public ActionResult Chat(int? id, int? usertype)//CustomerID or Delivery Man & Type
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
                    ViewBag.DeliveryMan_FullNAme = Customer.FirstName + " " + Customer.LastName;


                    int Deliveryman_ID = Convert.ToInt32(Session["id"]);
                    var messages = db.Messages.Where(msg => (msg.SenderID == Deliveryman_ID || msg.RecipientId == Deliveryman_ID) && (msg.SenderID == id || msg.RecipientId == id)).ToList();
                    if (messages != null)
                    {
                        return View(messages);

                    }
                    return View();
                }
                else if (usertype == 3)
                {
                    var DeliveryMan = db.Employees.Find(id);
                    if (DeliveryMan == null)
                    {
                        return HttpNotFound();
                    }
                    else
                    {
                        ViewBag.Receiver_UserName_onController = DeliveryMan.UserName;
                        ViewBag.Receiver_ID = id;
                        ViewBag.DeliveryMan_FullNAme = DeliveryMan.FirstName + " " + DeliveryMan.LastName;
                    }
                    int SupervisorId = Convert.ToInt32(Session["id"]);
                    //var requests = db.DeliveryRequests.Where(request => request.CustomerID == customerId && request.StatusID == 2).ToList();
                    var messages = db.Messages.Where(msg => (msg.SenderID == SupervisorId || msg.RecipientId == SupervisorId) && (msg.SenderID == id || msg.RecipientId == id)).ToList();

                    if (messages != null)
                    {
                        return View(messages);
                    }
                    else { return View(); }




                }
                else return HttpNotFound();
            }
            else return RedirectToAction("Index", "home");

        }
    }
   public class DeliveryMan{
        public int id { get; set; }
        public string fullName { get; set; }
        public string currentLocation { get; set; }

         }
    public class Delivery
    {
        public int requestID  { get; set; }
        public int deliveryManID { get; set; }
        public string currentLocation { get; set; }



    }
}
