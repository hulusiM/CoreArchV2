$(document).ready(function(){
	var arr = []; // List of users	
	$(document).on('click', '.msg_head', function() {	
		var chatbox = $(this).parents().attr("rel") ;
		$('[rel="'+chatbox+'"] .msg_wrap').slideToggle('slow');
		return false;
	});
	
	
	$(document).on('click', '.close', function() {	
		var chatbox = $(this).parents().parents().attr("rel") ;
		$('[rel="'+chatbox+'"]').hide();
		arr.splice($.inArray(chatbox, arr), 1);
		displayChatBox();
		return false;
	});
	
	$(document).on('click', '#sidebar-user-box', function() {
		var userID = $(this).attr("class");
	    var username = $(this).children().text() ;
	 
		 if ($.inArray(userID, arr) != -1)
		 {
	      arr.splice($.inArray(userID, arr), 1);
	     }
	 
		 arr.unshift(userID);
		 chatPopup =  '<div class="msg_box" style="right:270px" rel="'+ userID+'">'+
						'<div class="msg_head">'+username +
						'<div class="close">x</div> </div>'+
						'<div class="msg_wrap"> <div class="msg_body">	<div class="msg_push"></div> </div>'+
						'<div class="msg_footer"><textarea id="group_message_text" class="msg_input" rows="2"></textarea></div> 	</div> 	</div>' ;					
					
	     $("body").append(  chatPopup  );
		 displayChatBox();
	});
		
    function displayChatBox(){ 
	    i = 272 ; // start position
		j = 260;  //next position
		
		$.each( arr, function( index, value ) {  
		   if(index < 4){
	         $('[rel="'+value+'"]').css("right",i);
			 $('[rel="'+value+'"]').show();
		     i = i+j;			 
		   }
		   else{
			 $('[rel="'+value+'"]').hide();
		   }
        });		
	}

	$('.chat_head').click(function () {
		$('.chat_body').slideToggle('slow');
	});

});