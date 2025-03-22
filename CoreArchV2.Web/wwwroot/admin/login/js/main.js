
$(document).on('keypress', function (e) {
	if (e.which == 13) {
		checkLogin();
	}
});
$(document).on("click",
	".login100-form-btn",
	function (e) {
		checkLogin();
	});


$('.input-control').on('keypress', function (e) {
	setTimeout(function () {
		var valInput = $('#UserName').val();
		if (valInput.length == 2 && valInput == "(0") {
			valInput = $('#UserName').val('');
		}
	}, 10);


	var key = e.charCode || e.keyCode || 0;
	var phone = $(this);

	if (!(event.shiftKey == false && (key == 46 || key == 8 ||
		key == 37 || key == 39 || (key >= 48 && key <= 57)))) {
		event.preventDefault();
		ShowMessage("info", "Bilgi", "Sadece numara girilebilir");
	}

	if (phone.val().length === 0) {
		phone.val(phone.val() + '(');
	}
	// Auto-format- do not expose the mask as the user begins to type
	if (key !== 8 && key !== 9) {
		if (phone.val().length === 4) {
			phone.val(phone.val() + ')');
		}
		if (phone.val().length === 5) {
			phone.val(phone.val() + ' ');
		}
		if (phone.val().length === 9) {
			phone.val(phone.val() + '-');
		}
		if (phone.val().length >= 14) {
			phone.val(phone.val().slice(0, 13));
		}
	}

	// Allow numeric (and tab, backspace, delete) keys only
	return (key == 8 ||
		key == 9 ||
		key == 46 ||
		(key >= 48 && key <= 57) ||
		(key >= 96 && key <= 105));
}).on('focus', function () {
	phone = $(this);

	if (phone.val().length === 0) {
		phone.val('(');
	} else {
		var val = phone.val();
		phone.val('').val(val); // Ensure cursor remains at the end
	}
}).on('blur', function () {
	$phone = $(this);

	if ($phone.val() === '(') {
		$phone.val('');
	}
});


(function ($) {
	"use strict";


    /*==================================================================
    [ Validate ]*/
	var input = $('.validate-input .input100');

	$('.validate-form').on('submit', function () {
		var check = true;

		for (var i = 0; i < input.length; i++) {
			if (validate(input[i]) == false) {
				showValidate(input[i]);
				check = false;
			}
		}

		return check;
	});


	$('.validate-form .input100').each(function () {
		$(this).focus(function () {
			hideValidate(this);
		});
	});

	function validate(input) {
		if ($(input).attr('type') == 'email' || $(input).attr('name') == 'email') {
			if ($(input).val().trim().match(/^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{1,5}|[0-9]{1,3})(\]?)$/) == null) {
				return false;
			}
		}
		else {
			if ($(input).val().trim() == '') {
				return false;
			}
		}
	}

	function showValidate(input) {
		var thisAlert = $(input).parent();

		$(thisAlert).addClass('alert-validate');
	}

	function hideValidate(input) {
		var thisAlert = $(input).parent();

		$(thisAlert).removeClass('alert-validate');
	}



})(jQuery);