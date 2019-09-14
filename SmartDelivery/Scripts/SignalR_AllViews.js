//$(document).ready(function () {
var chathub_var;
$(function () {
    //$("#notificationsContent").hide();
    $("#chatnotifications").click(function () {
        var isopen = $(this).attr("data-ispoen");
        $("#notificationsCountValue").hide();
        if (isopen == "false") {
            $(this).attr("data-ispoen", "true");
            //$("#notificationsCountValue").show();
            $("#notificationsContent").show(200);
            $(".frame").show();
            $(".noti-icon i").css("color", "white");
        } else {
            $(this).attr("data-ispoen", "false");
            $("#notificationsContent").hide(200);
            $(".noti-icon i").css("color", "black");
            $(".frame").hide();
        }
    });
    
    /* when click esc hey close the notification popup */
    $(document).keyup(function (e) {
        var isopen = $("#chatnotifications").attr("data-ispoen");
        if (e.keyCode === 27) {
            if(isopen == "true"){
                $("#chatnotifications").attr("data-ispoen", "false");
                $("#notificationsContent").hide(200);   // esc
                $(".noti-icon i").css("color", "black");
                $(".frame").hide();
            }
        }
    });

    $("#frame").click(function () {
        $("#chatnotifications").attr("data-ispoen", "false");
        $("#notificationsContent").hide(300);
        $(".noti-icon i").css("color", "black");
        $(this).hide();
    });
    

    //____________________________________________________________________________________________end of deal with notification
    var connection = $.hubConnection();//create connection to proxy chatHub which is locted on the server
    connection.qs = { UserName: Myuser_name };
    chathub_var = connection.createHubProxy("SmartDeliveryHub");
    chathub_var.on("receiveMessage", onReceive); 
    chathub_var.on("add_unseenmsg_2notificationContent", onReceive_Unseen_msg);
    chathub_var.on("setnotificationcountvalue", Notification_Count_Value);
    chathub_var.on("logoff",OnLogOut);
    connection.logging = true;
    connection.start()// send request to server to open chanal with this client
    .done(function () { })
    .fail(function () { console.log('Could not connect i fail in start connection line : 80'); });
    connection.connectionSlow(function () { console.log("We're Currently try to solve your connection problem.") });  // to handle slow connection fire when your protocol detect that there are slow on connnection
});
//___________________________________________end of start connection & register client methodes & connections issues
var onReceive = function (msg, Senderid, SenderType, Sender_Full_name) {
    //$("#ulmessages").append("<li class='p-1 rounded mb-1'><div class='receive-msg'><!--<img src='demo/image1.jpg'>--><div class='receive-msg-desc  text-center mt-1 ml-1 pl-2 pr-2'><p class='pl-2 pr-2 rounded'>" + msg + "</p></div></div></li>");
    //append to notification
    var count = parseInt($("#notificationsCountValue").text());
    count++;
    $("#notificationsCountValue").text(count);
    $("#notificationsCountValue").show();
    if (MyType == 3) {//should put id & type in link
        $("#notificationsContent").append("<li class='li-of-msgs'>" + "<div>" + "<div class='sendername'>" + Sender_Full_name + "</div>" + "<div class='mesg'>" + msg + "</div>" + " </div>" + "</li>");
    }
    else {//put id only in link
        $("#notificationsContent").append("<li class='li-of-msgs'>" + "<div>" + "<div class='sendername'>" + Sender_Full_name + "</div>" + "<div class='mesg'>" + msg + "</div>" + " </div>" + "</li>");
    }

};
//____________________________________________________________________________________________end of receiveMessage
var onReceive_Unseen_msg = function (msg, sender_id,sent_date,Sender_Full_name){
    //$("#notificationsContent").append("<div>" + "<a href='<%: Url.Action('Chat','NormalCustomerController', new { id =sender_id}) %>'>" + "</a>" + "</div>");
    //var url = '@Url.Action("Chat", "NormalCustomer", new { id = "_id_" })'
    //var url = '<%= Url.Action("Chat", "NormalCustomer", new { id = "_id_" }) %>'
    //url = url.replace('_id_', sender_id);
    //$("#notificationsContent").append("<div>" + "<a href="+ url +">" + "<div>" + "<div>" + Sender_Full_name + "</div>" + "<div>" + msg + "</div>" + " </div>" + "</a>" + "</div>");
    //<a href='<%= Url.Action("MyAction", "MyController") %>'>Foo<a>
    //$("#notificationsContent").append("<a href=" +"'"+ url +"'"+ ">" + msg + "</a>");
    var Time = sent_date.substr(11, 5);
    if (MyType == 3) {//should put id & type in link
        $("#notificationsContent").append("<li style='position:relative;' class='li-of-msgs'>" + "<div>" + "</div>" + "<div>" + "<div class='sendername'>" + Sender_Full_name + "</div>" + "<div class='mesg'>" + msg + "</div>" + "<span  style='position: absolute; right: 11px; bottom: 11px; font-size: 12px; color: #0064ffba;'>" + Time + "</span>" + " </div>" + "</li>");
    }
    else {//put id only in link
        $("#notificationsContent").append("<li  style='position:relative;' class='li-of-msgs'>" + "<div>" + "</div>" + "<div>" + "<div class='sendername'>" + Sender_Full_name + "</div>" + "<div class='mesg'>" + msg + "</div>" + "<span style='position: absolute; right: 11px; bottom: 11px; font-size: 12px; color: #0064ffba;'>" + Time + "</span>" + " </div>" + "</li>");
    }
};
//____________________________________________________________________________________________end of onReceive_Unseen_msg
var Notification_Count_Value = function (Count_value) {
    $("#notificationsCountValue").text(Count_value);
    $("#notificationsCountValue").show();
};
//____________________________________________________________________________________________
var OnLogOut = function () {
    console.log("Log you out succssfully from SignalR_AllViews .")
    connection.stop();
};
//____________________________________________________________________________________________end of OnLogOut
//}); //end of $(document).ready(function ()

$(function () {
    $("#notification_Piece").click(function () {
        console.log("ana fe notification_Piece")
    });
})