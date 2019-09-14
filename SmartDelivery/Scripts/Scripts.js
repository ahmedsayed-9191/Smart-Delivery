$(function () {
    /*************************************/
    /*** header height ***/

    $(".header , .header .header-overlaying").height($(window).height());
    $(window).resize(function () {
        $(".header , .header .header-overlaying").height($(window).height());
    });
});

/*************************************/
/*** smoth scroll ***/
$(".navbar-nav li a").click(function () {
    $("html,body").animate({
        scrollTop: $("#" + $(this).data('value')).offset().top
    }, 1000);
});
/*************************************/
/*** scroll to top ***/

var scrollbutton = $(".up");
$(window).scroll(function () {
    if ($(this).scrollTop() >= 400) {
        scrollbutton.show();
    } else {
        scrollbutton.hide();
    }
});
scrollbutton.click(function () {
    $("html,body").animate({
        scrollTop: 0
    }, 800);
});
/*************************************/
/*** contact button***/

$(function () {
    $(".contact button").hover(function () {

        $(this).find("span").eq(0).animate({
            width: "100%"
        }, 500);

    }, function () {
        $(this).find("span").eq(0).animate({
            width: "0"
        }, 500);
    });
});
/*************************************/
/*** Done  notify***/
//$(function () {
//    $(".done-btn").click(function () {
//        $(".done-notification").animate({
//            right: "0"
//        }, 300);
//    });
//});
/*************************************/
/*** height of the chat-box***/
$(function () {
    var windowH = $(window).height();
    var navH = $(".navbar").innerHeight();
    var chat_headerH = $(".chat-header").innerHeight();
    var inputH = $(".chat-control").innerHeight();
    $(".messeges").height(windowH - (navH + chat_headerH + inputH));
});

/*************************************/
/*** height of content container***/
$(function () {
    var DocumentH = $(document).height();
    var navH = $(".navbar").innerHeight();
    //var chat_headerH = $(".chat-header").innerHeight();
    //var inputH = $(".chat-control").innerHeight();
    $(".content-container").height(DocumentH);
});




/*********************************************/
/**  **/


//$(function () {
//    $(".div-of-image").click(function () {
//        $(".offer-block").removeClass("col-sm-12");
//        $(".image-section").removeClass("col-sm-2");
//        $(this).removeClass("div-of-image").addClass("image-overlay");
//        $(this).height = $(window).height();
//        $("body").css("overflow", "hidden");

//});
//});