using System;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using SmartDelivery.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Entity;

namespace SmartDelivery.HUBs
{
    public class SmartDeliveryHub : Hub
    {
        private SmartDeliveryEntities db = new SmartDeliveryEntities();
        static ConcurrentDictionary<string, string > Online_Users = new ConcurrentDictionary<string, string>();
        //Debug.Print("User_ID In HUB Is  : "+Sender_ID);
        //clients will call it & send message to it
        //Context.Connection.GetHttpContext().Abort();
        [Authorize]// no one can send message if he not Authorized
        public void SendMessage(string message, string User_Name,int Receiver_ID)
        {
            //var User = MyUsers.FirstOrDefault(x => x.Key == Receiver_UserName.ToString());
            //var deliveryman = db.Employees.Find(Receiver_ID);
            //send Message to another person
            //Clients.Client(Context.ConnectionId).receiveMessage(message);
            //Clients.Client(User.Value).receiveMessage(message);
            //string Receiver_ID_String = Receiver_ID.ToString();
            int Sender_ID = Convert.ToInt32(Controllers.Business.GetLoginCookie());
            int User_Type = Convert.ToInt32(Controllers.Business.GetLogin_UserType_Cookie());
            string sender_FullName;
            Message myMessage = new Message();
            myMessage.Sent_DateTime = DateTime.Now;
            var Msg_Date = myMessage.Sent_DateTime.ToString("h:mm tt"); // 7:00 AM // 12 hour clock;
            if (User_Type == 4 || User_Type == 5)
            {
                var customer = db.Customers.Find(Sender_ID);
                sender_FullName = customer.FirstName + " " + customer.LastName;
            }
            else
            {
                var employee = db.Employees.Find(Sender_ID);
                sender_FullName = employee.FirstName + " " + employee.LastName;
            }
            Clients.Group(User_Name).receiveMessage(message, Sender_ID, User_Type, sender_FullName, Msg_Date);
            //Clients.Client(Context.ConnectionId).receiveMessage("Sent");
            //Clients.Client(User.Value).receiveMessage("New Message");

            //int User_Type = Convert.ToInt32(Controllers.Business.GetLogin_UserType_Cookie());
            //add customer's Message to database
            
            myMessage.SenderID = Sender_ID;
            myMessage.RecipientId = Receiver_ID;
            myMessage.MessageContent = message;
            myMessage.Seen = false;
            db.Messages.Add(myMessage);
            db.SaveChanges();
            
        }
        public void Set_message_asseen(int Message_Receiver_ID) {
            int My_ID = Convert.ToInt32(Controllers.Business.GetLoginCookie());
            var Unseen_Messages = db.Messages.Where(Message => Message.RecipientId == My_ID && Message.SenderID == Message_Receiver_ID&& Message.Seen == false).ToList();
            if (Unseen_Messages != null)
            {
                //Message unseen_msg = new Message();
                foreach (Message unseen_msg in Unseen_Messages)
                {
                    unseen_msg.Seen = true;
                    unseen_msg.Seen_DateTime= DateTime.Now;
                    db.Entry(unseen_msg).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }
        public async override Task OnConnected()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                if (Online_Users.ContainsKey(Context.User.Identity.Name)) {
                    // user are alreedy on online list should remove  him & add again to update his connection_Id
                    //var name = Online_Users.FirstOrDefault(x => x.Key == Context.User.Identity.Name);
                    //string s;
                    //Online_Users.TryRemove(name.Key, out s);
                   // Online_Users.TryAdd(Context.User.Identity.Name, Context.ConnectionId);
                    //Clients.Caller.differentName();
                }
                else
                {
                    ///Online_Users.TryAdd(Context.User.Identity.Name,Context.ConnectionId);
                    int Sender_ID = Convert.ToInt32(Controllers.Business.GetLoginCookie());
                    // if 2 request come wait one of them
                    await Groups.Add(Context.ConnectionId,Context.User.Identity.Name);
                    //Clients.All.receiveMessage($"{Context.ConnectionId}:Joined To {Context.User.Identity.Name}");
                   // foreach (KeyValuePair<String, String> user in Online_Users)
                    //{
                        //should tell him with all online users
                        //Clients.Caller.online(user.Key);
                    //}
                    // should tell all other that this user is added and be online
                    //Clients.Others.enters(Context.User.Identity.Name);
                }
                //Clients.Client(Context.ConnectionId).notifyuser($" You  :  Connected With CID =>    {Context.ConnectionId} And UserName :  {Context.User.Identity.Name}");
                //Clients.AllExcept(Context.ConnectionId).receiveMessage($"{Context.User.Identity.Name}:Connected With CID =>    {Context.ConnectionId}");
                //Clients.Client(Context.ConnectionId).receiveMessage($" You  :  Connected With CID =>    {Context.ConnectionId} And UserName :  {Context.User.Identity.Name}");
                Clients.AllExcept(Context.ConnectionId).receiver_status(Context.User.Identity.Name, true);
                Get_all_unseen_messages();
            }
            //else
            //{
            //    if (Context.User.Identity.Name == null)
            //    {
            //        //OnDisconnected(true);
            //    }
            //    Clients.Caller.receiveMessage("Unkown user Connected");
            //}
            //int Sender_ID = Convert.ToInt32(Controllers.Business.GetLoginCookie());
            //int User_Type = Convert.ToInt32(Controllers.Business.GetLogin_UserType_Cookie());
            //return base.OnConnected();
        }

        //_______________________________________________________________________
        public override Task OnReconnected()
        {
            //هيعمل ريكونكت بنفس الكونكشن اي دي 
            //فاير لو بعد 20 ثواني الكيب الايف مجاتش
           // Clients.All.receiveMessage($"{Context.ConnectionId}:Reconnected");
            return base.OnReconnected();
        }
        //______________________________________________________________________________
        public override Task OnDisconnected(bool stopCalled) //stopCalled = true if stop is called
        {
            //var name = Online_Users.FirstOrDefault(x => x.Value == Context.ConnectionId.ToString());
            //string s =" ";
            //Online_Users.TryRemove(name.Key, out s); //true if the object was removed successfully; otherwise, false.
            //should tell all other that this user are disconected
            //return Clients.All.disconnected(name.Key);
            //return Clients.All.receiveMessage($"{Context.User.Identity.Name}:DisConnected With CID =>    {Context.ConnectionId}");
            //if (stopCalled)
            //{
            //    Console.WriteLine(String.Format("Client {0} explicitly closed the connection.", Context.ConnectionId));
            //}
            //else
            //{
            //    Console.WriteLine(String.Format("Client {0} timed out .", Context.ConnectionId));
            //}
            // int Sender_ID = Convert.ToInt32(Controllers.Business.GetLoginCookie());
            //int User_Type = Convert.ToInt32(Controllers.Business.GetLogin_UserType_Cookie());   
            Clients.AllExcept(Context.ConnectionId).receiver_status(Context.User.Identity.Name, false);
            return base.OnDisconnected(stopCalled);
        }
        //public override async Task OnDisconnectedAsync(Exception exception)
        //{
        //    await base.OnDisconnectedAsync(exception);
        //    //Logger.Debug("A client disconnected from MyChatHub: " + Context.ConnectionId);
        //}

        //____________________________________________________________________________
        //public string Get_ConnectionId_ForUser(string username)
        //{
        //    var name = Online_Users.FirstOrDefault(x => x.Key == username);
        //    string User_ConnectionId =name.Value.ToString();
        //    return User_ConnectionId;
        // }
        //____________________________________________________________________________
        //public void PushData()
        //{
        //    //Values is copy-on-read but Clients.Clients expects IList, hence ToList()
        //    Clients.Clients(MyUsers.Keys.ToList()).ClientBoundEvent(data);
        //}
        //__________________________________________________________________________
        //_____________________________________________________________________________________
        //public void Send_Test_Message(string message)
        //{
        //    Clients.Client(Context.ConnectionId).receiveMessage(message);
        //    Clients.Client(Context.ConnectionId).receiveMessage("Sent");
        //}
        //___________________________________________C:\Users\Ahmed\Documents\Visual Studio 2015\Projects\SmartDelivery\SmartDelivery\Startup.cs____________________________________________
        public  void Get_all_unseen_messages()
        {
            int My_ID = Convert.ToInt32(Controllers.Business.GetLoginCookie());
            int notification_count = 0;
            var Unseen_Messages = db.Messages.Where(Message => Message.RecipientId== My_ID && Message.Seen==false).ToList();
            if (Unseen_Messages != null)
            {
                //Message unseen_msg = new Message();
                foreach (Message unseen_msg in Unseen_Messages)
                {
                  //ustomer Customer_Obj = new Customer();
                    var Customer_Obj = db.Customers.Find(unseen_msg.SenderID);
                    string Sender_Full_name;
                    if (Customer_Obj != null)
                    {
                         Sender_Full_name = Customer_Obj.FirstName + "  " + Customer_Obj.LastName;
                    }
                    else {
                        var Employee_Obj = db.Employees.Find(unseen_msg.SenderID);
                         Sender_Full_name = Employee_Obj.FirstName + "  " + Employee_Obj.LastName;
                    }
                    Clients.Caller.add_unseenmsg_2notificationContent(unseen_msg.MessageContent, unseen_msg.SenderID, unseen_msg.Sent_DateTime, Sender_Full_name);
                    notification_count++;
                }
            }
            Clients.Caller.setnotificationcountvalue(notification_count);
        }
    }
}