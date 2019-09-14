using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartDelivery.Models;
using System.Diagnostics;

namespace MyCard.Controllers
{
    public class PaypalController : Controller
    {
        
     
        public ActionResult ConfirmDeliveryRequestBayment()
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Index", "home");
            }
            Session["PaymentFlag"] = true;

            return View();
        }

        public bool checkPayment()
        {
           
            if (Session["PaymentFlag"] != null)
                return true;
            else return false;
        }

        

  public ActionResult ConfirmOrderBayment()
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Index", "home");
            }
            Session["OrderPaymentFlag"] = true;

            return View();
        }

        public bool checkOrderPayment()
        {

            if (Session["OrderPaymentFlag"] != null)
                return true;
            else return false;
        }
    }
}