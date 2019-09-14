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
    public class AdminController : Controller
    {
        private SmartDeliveryEntities db = new SmartDeliveryEntities();

        public ActionResult getTopRated()
        {
            if (Session["id"] != null)
            {
                var rets = db.Rates.OrderByDescending(r => (r.EmpRate / r.RequestsNumber)).Take(10).ToList();

                if (rets != null)
                {
                    return View(rets);
                }
                else
                {
                    return View();
                }
            }
            else return RedirectToAction("Index", "home");          
        }

      
        // GET: Admin
        public ActionResult Index(int? id , int? usertype)
        {
            if (!HomeController.Authenticated && usertype != 1)
            {
                return RedirectToAction("Index", "home");
            }

            if (id != null)
            {
                Employee employee = db.Employees.Find(id);
                Session["userName"] = employee.UserName;
                Session["userType"] = employee.UserType.ToString();
                Session["id"] = id;
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
        /////////////////// admin profile \\\\\\\\\\\\\\\\\\\\

        [HttpGet]
        public ActionResult ViewProfile()
        {
            if (Session["id"] != null)
            {
                int ID = int.Parse(Session["id"].ToString());

                var admin = db.Employees.FirstOrDefault(n => n.ID == ID);

                if (admin != null)
                {
                    return View(admin);
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

                int ID = Convert.ToInt32(Session["id"]);

                var admin = db.Employees.FirstOrDefault(s => s.ID == ID);
                if (admin == null)
                {
                    return HttpNotFound();
                }
                Session["photo"] = admin.Photo;
                Session["userName"] = admin.UserName;
                Session["oldPass"] = admin.PassWord;
                return View(admin);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult UpdateProfile(Employee admin, HttpPostedFileBase image)
        {
            if (image != null)
            {
                admin.Photo = new byte[image.ContentLength];
                image.InputStream.Read(admin.Photo, 0, image.ContentLength);
            }
            else
            {
                admin.Photo = (byte[])Session["photo"];
            }

            try
            {

                string oldUserName = Session["userName"].ToString();

                //Check if old username equal to the new or not
                if (admin.UserName != oldUserName)
                {
                    //check the username of the employee if exits or not 
                    Customer checkEmployeeExistInCustomers = new Customer();
                    checkEmployeeExistInCustomers = db.Customers.FirstOrDefault(emp => emp.UserName == admin.UserName);

                    Employee checkEmployeeExistInEmployees = new Employee();
                    checkEmployeeExistInEmployees = db.Employees.FirstOrDefault(emp => emp.UserName == admin.UserName);

                    if (checkEmployeeExistInCustomers != null || checkEmployeeExistInEmployees != null)
                    {
                        Response.Write("<script>alert('This UserName Is Not Available Type Another One !.');</script>");
                        return View(admin);
                    }
                }
                if (admin.PassWord != Session["oldPass"].ToString())
                {
                    admin.PassWord = Encryption.Encrypt(admin.PassWord);
                }

                db.Entry(admin).State = EntityState.Modified;
                db.SaveChanges();
                Response.Write("<script>alert('Customer Updated Successfully .');</script>");

                return RedirectToAction("ViewProfile");
            }
            catch
            {
                return View(admin);
            }
        }
        /////////////////////////////////////
        /// <summary>
        /// Shift Managament //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// 
        public ActionResult ListAllShifts()
        {
            if (Session["id"] != null)
            {
                var shifts = db.Shifts.ToList();
                return View(shifts);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult AddShift()
        {
            if (Session["id"] != null)
            {
                return View();
            }
            else return RedirectToAction("Index", "home");
           
        }

        [HttpPost]
        public ActionResult AddShift(Shift shift)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Shifts.Add(shift);
                    db.SaveChanges();
                    return RedirectToAction("ListAllShifts");
                }

                return View(shift);
            }
            catch
            {
                return View(shift);
            }
        }

        [HttpGet]
        public ActionResult EditShift(int? shiftId)
        {
            if (Session["id"] != null)
            {
                var shift = db.Shifts.Find(shiftId);
                if (shift == null)
                {
                    return HttpNotFound();
                }
                return View(shift);
            }
            else return RedirectToAction("Index", "home");

        }
        [HttpPost]
        public ActionResult EditShift(Shift shift)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(shift).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("ListAllShifts");
                }
                return View(shift);
            }
            catch
            {
                return View(shift);
            }
        }

        [HttpGet]
        public ActionResult DeleteShift(int? shiftId)
        {
            if (Session["id"] != null)
            {
                var shift = db.Shifts.Find(shiftId);
                if (shift == null)
                {
                    return HttpNotFound();
                }
                return View(shift);

            }
            else return RedirectToAction("Index", "home");

        }

        [HttpPost]
        public ActionResult DeleteShift(Shift _shift)
        {
            try
            {
                var myShift = db.Shifts.Find(_shift.ID);
                db.Shifts.Remove(myShift);
                db.SaveChanges();
                return RedirectToAction("ListAllShifts");
               

            }
            catch
            {
                return View(_shift);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        public ActionResult ListAllCustomers()
        {
            if (Session["id"] != null)
            {
                //List all employess with usertype "2"
                var customers = db.Customers.ToList();
                return View(customers);
            }
            else return RedirectToAction("Index", "home");


        }

        /// <summary>
        /// SuperVisor Managament //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// 

        public ActionResult ListAllSuperVisor()
        {
            if (Session["id"] != null)
            {
                //List all employess with usertype "2"
                var supervisors = db.Employees.Where(s => s.EmployeeType == 2).ToList();
                return View(supervisors);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult AddSuperVisor()
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
        public ActionResult AddSuperVisor(Employee employee, HttpPostedFileBase image)
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
                    Response.Write("<script>alert('This UserName Is Not Available try with Another One !.');</script>");
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
                myEmployee.Authorized = 1;
                myEmployee.Active = 0;
                myEmployee.EmployeeType = 2; // => "2" the type of the supervisor
                myEmployee.ShiftID = employee.ShiftID;
                myEmployee.HolidayID = employee.HolidayID;

                db.Employees.Add(myEmployee);
                try
                {
                    db.SaveChanges();
                } catch(Exception ex)
                {
                    Response.Write("<script>alert('invalid Data! Try again.');</script>");
                    return View(employee);
                }
                

                Response.Write("<script>alert('" + employee.FirstName + " " + employee.LastName + " Added Successfully.');</script>");
                TempData["msg10"] = "<script>alert('" + employee.FirstName + " " + employee.LastName + " Added Successfully.');</script>";


                return RedirectToAction("ListAllSuperVisor");
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
        public ActionResult DetailsOfSuperVisor(int? id)
        {
            if (Session["id"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Employee supervisor = db.Employees.FirstOrDefault(S => S.ID == id);

                if (supervisor == null)
                {
                    return HttpNotFound();
                }

                return View(supervisor);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult UpdateSuperVisor(int? id)
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
                Employee supervisor = db.Employees.FirstOrDefault(S => S.ID == id);
                if (supervisor == null)
                {
                    return HttpNotFound();
                }
                Session["photo"] = supervisor.Photo;
                Session["userName"] = supervisor.UserName;
                Session["oldPass"] = supervisor.PassWord;
                return View(supervisor);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult UpdateSuperVisor(Employee supervisor , HttpPostedFileBase image )
        {
            //List Of Days 
            ViewBag.HolidayID = new SelectList(db.Days.ToList(), "ID", "DayName");
            //List Of Shifts
            ViewBag.ShiftID = new SelectList(db.Shifts.ToList(), "ID", "StartTime");

            if (image != null)
            {
                supervisor.Photo = new byte[image.ContentLength];
                image.InputStream.Read(supervisor.Photo, 0, image.ContentLength);  
            }
            else
            {
                supervisor.Photo = (byte[])Session["photo"];
            }

            try
            {

                string oldUserName = Session["userName"].ToString(); 

                //Check if old username equal to the new or not
                if (supervisor.UserName != oldUserName)
                {
                    //check the username of the employee if exits or not 
                    Customer checkEmployeeExistInCustomers = new Customer();
                    checkEmployeeExistInCustomers = db.Customers.FirstOrDefault(emp => emp.UserName == supervisor.UserName);

                    Employee checkEmployeeExistInEmployees = new Employee();
                    checkEmployeeExistInEmployees = db.Employees.FirstOrDefault(emp => emp.UserName == supervisor.UserName);

                    if (checkEmployeeExistInCustomers != null || checkEmployeeExistInEmployees != null)
                    {
                        Response.Write("<script>alert('This UserName Is Not Available Type Another One !.');</script>");
                        return View(supervisor);
                    }
                }
                if(supervisor.PassWord != Session["oldPass"].ToString())
                {
                    supervisor.PassWord = Encryption.Encrypt(supervisor.PassWord);
                }
                db.Entry(supervisor).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('invalid Data! Try again.');</script>");
                    return View(supervisor);
                }

              
                Response.Write("<script>alert('Supervisor Updated Successfully .');</script>");

                return RedirectToAction("ListAllSuperVisor");
            }
            catch
            {
                return View(supervisor);
            }
                       
        }

        [HttpPost]
        public ActionResult PanEmployee(int? id ,int? complaintFlag)
        {
            var Emp = db.Employees.FirstOrDefault(e => e.ID == id);
            if (Emp == null)
            {
                var customer = db.Customers.FirstOrDefault(e => e.ID == id);
                if(customer == null)
                return HttpNotFound();
                else
                {
                    customer.Authorized = 0;
                    db.SaveChanges();
                    return RedirectToAction("ListAllCustomers");
                }
            }
            else
            {
                    try
                    {
                        Emp.Authorized = 0;
                        db.SaveChanges();

                        //Check if "pan action" ==> requested from "comlaints View"
                        if (complaintFlag == 1)
                        {
                            return RedirectToAction("ListAllComplaints");
                        }

                        if (Emp.EmployeeType == 2)
                        {
                            return RedirectToAction("ListAllSuperVisor");
                        }
                        else
                        {
                            return RedirectToAction("ListAllDeliveyMen");
                        }
                        
                    }
                    catch (Exception ex)
                    {
                    Debug.WriteLine("Error !"+ ex.Message);
                    return RedirectToAction("ListAllDeliveyMen");
                }   
                
                 
            }
            
        }

        [HttpPost]
        public ActionResult unPanEmployee(int? id, int? complaintFlag)
        {
            var Emp = db.Employees.FirstOrDefault(e => e.ID == id);
            if (Emp == null)
            {
                var customer = db.Customers.FirstOrDefault(e => e.ID == id);
                if (customer == null)
                    return HttpNotFound();
                else
                {
                    customer.Authorized = 1;
                    db.SaveChanges();
                    return RedirectToAction("ListAllCustomers");
                }
            }
            else
            {
                try
                {
                    Emp.Authorized = 1;
                    db.SaveChanges();

                    //Check if "unPan action" ==> requested from "comlaints View"
                    if (complaintFlag == 1)
                    {
                        return RedirectToAction("ListAllComplaints");
                    }

                    if (Emp.EmployeeType == 2)
                    {
                        return RedirectToAction("ListAllSuperVisor");
                    }
                    else
                    {
                        return RedirectToAction("ListAllDeliveyMen");
                    }
                }
                catch
                {
                    return View(Emp);
                }
            }

        }

        /////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// DeliveryMan Managament //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// 

        [HttpGet]
        public ActionResult ListAllDeliveyMen()
        {
            if (Session["id"] != null)
            {
                //List all employess with usertype "3"
                //all delivery men without "need approval(2)" and "refused(3)"
                var deliveryMen = db.Employees.Where(s => s.EmployeeType == 3 && s.Authorized != 2 && s.Authorized != 3).ToList();
                return View(deliveryMen);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult ListAllWaitingDeliveyMenForApproval()
        {
            if (Session["id"] != null)
            {
                //List all employess with usertype "3"
                //Which need to approval =>>> with tag "2"
                var deliveryMen = db.Employees.Where(s => s.EmployeeType == 3 && s.Authorized == 2).ToList();
                return View(deliveryMen);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult DetailsOfDeliveyMen(int? id)
        {
            if (Session["id"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Employee deliveyMen = db.Employees.FirstOrDefault(D => D.ID == id);
                if (deliveyMen == null)
                {
                    return HttpNotFound();
                }
                return View(deliveyMen);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult AcceptRequest(int? id)
        {
            var Emp = db.Employees.FirstOrDefault(e => e.ID == id);
            if (Emp == null)
            {
                return HttpNotFound();
            }
            else
            {
                try
                {
                    Emp.Authorized = 1;
                    db.SaveChanges();
                    return RedirectToAction("ListAllWaitingDeliveyMenForApproval");
                }
                catch
                {
                    return View(Emp);
                }
            }

        }

        [HttpPost]
        public ActionResult RefuseRequest(int? id)
        {
            var Emp = db.Employees.FirstOrDefault(e => e.ID == id);
            if (Emp == null)
            {
                return HttpNotFound();
            }
            else
            {
                try
                {
                    Emp.Authorized = 3; // "3" means Refused from System
                    db.SaveChanges();
                    return RedirectToAction("ListAllWaitingDeliveyMenForApproval");
                }
                catch
                {
                    return View(Emp);
                }
            }

        }

        [HttpGet]
        public ActionResult ListAllComplaints()
        {
            if (Session["id"] != null)
            {
                var complaints = db.Complaints.ToList();
                return View(complaints);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult DetailsOfTheComplaint(int? id)
        {
            if (Session["id"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Complaint complaint = db.Complaints.FirstOrDefault(C => C.ID == id);

                if (complaint == null)
                {
                    return HttpNotFound();
                }

                return View(complaint);
            }
            else return RedirectToAction("Index", "home");
        }

        /////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Shipment type Managament //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// 
        public ActionResult ListAllShipmentTypes()
        {
            if (Session["id"] != null)
            {
                var ShipmentTypes = db.ShipmentTypes.ToList();
                return View(ShipmentTypes);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpGet]
        public ActionResult AddShipmentType()
        {
            if (Session["id"] != null)
            {
                return View();
            }
            else return RedirectToAction("Index", "home");    
        }

        [HttpPost]
        public ActionResult AddShipmentType(ShipmentType shipmentType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.ShipmentTypes.Add(shipmentType);
                    db.SaveChanges();
                    return RedirectToAction("ListAllShipmentTypes");
                }

                return View(shipmentType);
            }
            catch
            {
                return View(shipmentType);
            }
        }

        [HttpGet]
        public ActionResult EditShipmentType(int? shipmentTypeId)
        {
            if (Session["id"] != null)
            {
                var shipmentType = db.ShipmentTypes.Find(shipmentTypeId);
                if (shipmentType == null)
                {
                    return HttpNotFound();
                }
                return View(shipmentType);
            }
            else return RedirectToAction("Index", "home");
        }
        [HttpPost]
        public ActionResult EditShipmentType(ShipmentType shipmentType)
        {
            try
            {
                
                  
                    db.Entry(shipmentType).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("ListAllShipmentTypes");
                
                
            }
            catch
            {
                Response.Write("<script>alert('invalid Data! Try again.');</script>");
                return View(shipmentType);
            }
        }

        [HttpGet]
        public ActionResult DeleteShipmentType(int? shipmentTypeId)
        {
            if (Session["id"] != null)
            {
                var shipmentType = db.ShipmentTypes.Find(shipmentTypeId);
                if (shipmentType == null)
                {
                    return HttpNotFound();
                }
                return View(shipmentType);
            }
            else return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult DeleteShipmentType(ShipmentType shipmentType)
        {
            try
            {
                var myShipmentType = db.ShipmentTypes.Find(shipmentType.ID);
                db.ShipmentTypes.Remove(myShipmentType);
                db.SaveChanges();
                return RedirectToAction("ListAllShipmentTypes");
            }
            catch
            {
                return View(shipmentType);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////


    }

}