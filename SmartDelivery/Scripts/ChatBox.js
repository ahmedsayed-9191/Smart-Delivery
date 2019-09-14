//$(document).ready(function () {
var chathub_var;
$(function () {
    //$("#notificationsContent").hide();
    $("#chatnotifications").click(function () {
        $(".notify-counter").hide();
        var isopen = $(this).attr("data-ispoen");
        if (isopen == "false") {
            $(this).attr("data-ispoen", "true");
            //$("#notificationsCountValue").show(500);
            $("#notificationsContent").show(300);
            $(".frame").show();
        } else {
            $(this).attr("data-ispoen", "false");
            $("#notificationsContent").hide(300);
            $(".frame").hide();
        }
    }); //end of click function

    /* when click esc hey close the notification popup */
    $(document).keyup(function (e) {
        var isopen = $("#chatnotifications").attr("data-ispoen");
        if (e.keyCode === 27) {
            if (isopen == "true") {
                $("#chatnotifications").attr("data-ispoen", "false");
                $("#notificationsContent").hide(200);   // esc
                $(".frame").hide();
            }
        }
    });

    $("#frame").click(function () {
        $("#chatnotifications").attr("data-ispoen", "false");
        $("#notificationsContent").hide(300);
        $(this).hide();
    });
    /*********************************************/
    /** get the bottom of the chat when open the page **/
    $(function () {
        var chatbox = document.getElementById("chat-box-content");
        chatbox.scrollTop = chatbox.scrollHeight;
    });

    //__________________________________________________________________end of deal with notification
    $("#textmeesage").click(function () {
        var Receiver_ID_onJS = $("#receiver_id_onHtml").val();
        chathub_var.invoke("set_message_asseen", Receiver_ID_onJS)
        .done(function () { console.log("Seen Updated Successfully"); })
        .fail(function () { console.log("Seen Not Updated") });
    })
    var connection = $.hubConnection(); //create connection to proxy chatHub which is locted on the server
    connection.qs = { UserName: Myuser_name };
    chathub_var = connection.createHubProxy("SmartDeliveryHub");
    chathub_var.on("receiveMessage", onReceive);//when client search for a finction called " receiveMessage" go to the callback "OnReceive"
    chathub_var.on("add_unseenmsg_2notificationContent", onReceive_Unseen_msg);
    chathub_var.on("setnotificationcountvalue", Notification_Count_Value);
    chathub_var.on("logoff",OnLogOut);
    connection.logging = true;
    connection.start()    // send request to server to open chanal with this client
    .done(function () {
        //$("#ulmessages").append("<span>You Are Connected</span>");
    })
    .fail(function () { console.log('Could not connect i fail in start connection line : 80'); });
    // to handle slow connection fire when your protocol detect that there are slow on connnection
    connection.connectionSlow(function () { console.log("We're Currently try to solve your connection problem.") });
});
//_________________________________________________________________________________________________end of start connection & register client methodes & connections issues
var onReceive = function (msg, Senderid, SenderType, Sender_Full_name, Msg_sent_datetime) {
    // Sender can send a script as a msg so i take from it onlt text & html

    //var encodedmsg = $('<div />').text(msg).html();
    var Receiver_ID_onJS= $("#receiver_id_onHtml").val();
    if (Senderid == Receiver_ID_onJS) {

    $("#ulmessages").append("<li><div class='receive-msg'><div class=''><p>" + msg + "</p></div></div></li>");

        //get the bottom of the chat when send the message
    var chatbox = document.getElementById("chat-box-content");
    chatbox.scrollTop = chatbox.scrollHeight;
    //$("#ulmessages").append("<li><div class='receive-msg'><div class='text-center'><p>" + Msg_sent_datetime + "</p></div></div></li>");

   
    }
    //console.log(userName + ' ' + message);
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
var OnLogOut = function () {
    console.log("Log you out succssfully from ChatBox .")
    connection.stop();
};
//____________________________________________________________________________________________end of OnLogOut
var onSend = function () {
    if ($(input).val() == "") { }// when input empty do nothing
    else
    {
        //var rex = new RegExp(/Test[0-9]+/gi);
        var msg = $("#textmeesage").val();
        if (msg.match(/^[A-Za-z\._\-0-9\s\w\d\n\t\?\!]*$/gim) || msg.match(/^[\u0600-\u06ff]|[\u0750-\u077f]|[\ufb50-\ufc3f]|[\ufe70-\ufefc]+$/gim)) //english or arabic
        {
            var Receiver_UserName_onJS = $("#receiver_UserName_onHtml").val();
            //var Receiver_ConnectionID = chathub_var.invoke("get_ConnectionId_ForUser", Receiver_UserName_onJS)
            //.done(function () { console.log('Get Customer Conectuion ID Succsfullly In Line : 98, connection ID =' + Receiver_ConnectionID); })
            //.fail(function () { console.log('Could not Get Customer Connection ID Line : 99'); });
            var Receiver_ID_onJS = $("#receiver_id_onHtml").val();
            chathub_var.invoke("sendMessage", msg, Receiver_UserName_onJS, Receiver_ID_onJS)
            .done(function () {
                $("#ulmessages").append("<li class='bg-primary  send-msg' style='position: relative;'>"
         + msg +
         "<i title = 'Sent' class='fa fa-check-circle'></i></li>");

                //$("#ulmessages").append("<li class='Sent-Successfully'>" + " Sent Successfully " + "</li>");

                //get the bottom of the chat when send the message
                var chatbox = document.getElementById("chat-box-content");
                chatbox.scrollTop = chatbox.scrollHeight;
            })
            .fail(function () {
                $("#ulmessages").append("<li class='bg-primary send-msg'>" + "Error in Sending Meesage try again !" + "</li>");
            });// if fail to send msg

            $(input).val(""); // clear input field
            
        }
        else
        {
            $("#ulmessages").append("<li style=''>" + "Please enter a valid Meesage!" + "</li>");
        }
    }
}
//____________________________________________________________________________________________end of onSend

var onReceive_Unseen_msg = function (msg, sender_id, sent_date, Sender_Full_name) {
    //$("#notificationsContent").append("<div>" + "<a href='<%: Url.Action('Chat','NormalCustomerController', new { id =sender_id}) %>'>" + "</a>" + "</div>");
    //$("#onlineList").append("<div>" + "<a href='<%: Url.Action('Chat','NormalCustomerController', new { id =sender_id}) %>'>" + "</a>" + "</div>");
    //$("#notificationsContent").append("<li>" + "<a href='https://www.facebook.com/messages/t/100010481503383'>" + "</a>" + "</li>");
    //var $newLink = $('<a href="@Url.Action("Chat")?id=' + sender_id + '">' + msg + '</a>');
    var Time = sent_date.substr(11, 5);
    if (MyType == 3) {//should put id & type in link
        $("#notificationsContent").append("<li style='position:relative;' class='li-of-msgs'>" + "<div>" + "<div class='sendername'>" + Sender_Full_name + "</div>" + "<div class='mesg'>" + msg + "</div>" + "<span style='position: absolute; right: 11px; bottom: 11px; font-size: 12px; color: #0064ffba;'>" + Time + "</span>" + " </div>" + "</li>");
    }
    else {//put id only in link
        $("#notificationsContent").append("<li style='position:relative;' class='li-of-msgs'>" + "<div>" + "<div class='sendername'>" + Sender_Full_name + "</div>" + "<div class='mesg'>" + msg + "</div>" + "<span style='position: absolute; right: 11px; bottom: 11px; font-size: 12px; color: #0064ffba;'>" + Time + "</span>" + " </div>" + "</li>");
    }
};
//____________________________________________________________________________________________end of onReceive_Unseen_msg
var Notification_Count_Value = function (Count_value) {
    console.log("Count_value Is" + Count_value);
    $("#notificationsCountValue").text(Count_value);
    $("#notificationsCountValue").show();
};
//____________________________________________________________________________________________end of Notification_Count_Value
var Receiver_Status = function (Received_Receiver_Username, Status) {
    var Receiver_UserName_onJS = $("#receiver_UserName_onHtml").val();
    if (Received_Receiver_Username == Receiver_UserName_onJS) {
        if (Status == true) {
            //set green color
            //$(".status-of-the-user").css("background", "green");
            console.log("green");
        }
        else {
            if (Status == false) {
                //set red color
                //$(".status-of-the-user").css("background", "red");
                console.log("red");
            }
        }


    }//
};
//_______________________________________________________________________________________end of receiver status
//}); //end of $(document).ready(function ()
