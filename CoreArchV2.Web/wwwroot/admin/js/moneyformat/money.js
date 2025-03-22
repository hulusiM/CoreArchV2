//12.52,22 -->virgülden sonra 2 hane
$(".moneyCurrency").on({
    keyup: function () {
        formatCurrency($(this));
    },
    //blur: function () {
    //    formatCurrency($(this), "blur");
    //},
    change: function () {
        formatCurrency($(this));
    },
    paste: function () {
        formatCurrency($(this));
    }
});

function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

function formatCurrency(input, blur) {
    var input_val = input.val();
    if (input_val === "") { return; }

    var original_len = input_val.length;
    var caret_pos = input.prop("selectionStart");

    if (input_val.indexOf(",") >= 0) {
        var decimal_pos = input_val.indexOf(",");

        var left_side = input_val.substring(0, decimal_pos);
        var right_side = input_val.substring(decimal_pos);

        left_side = formatNumber(left_side);

        right_side = formatNumber(right_side);

        if (blur === "blur")
            right_side += "00";

        right_side = right_side.substring(0, 2);
        input_val = left_side + "," + right_side;

    } else {
        input_val = formatNumber(input_val);
        input_val = input_val;

        if (blur === "blur") {
            input_val += ",00";
        }
    }

    input.val(input_val);

    var updated_len = input_val.length;
    caret_pos = updated_len - original_len + caret_pos;
    input[0].setSelectionRange(caret_pos, caret_pos);
}


/////////////////////////////////////////////////////////////////////
$(".moneyCurrency5Places").on({
    keyup: function () {
        formatCurrency5($(this));
    },
    //blur: function () {
    //    formatCurrency($(this), "blur");
    //},
    change: function () {
        formatCurrency5($(this));
    },
    paste: function () {
        formatCurrency5($(this));
    }
});


function formatNumber5(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{5})+(?!\d))/g, ".")
}

function formatCurrency5(input, blur) {
    var input_val = input.val();
    if (input_val === "") { return; }

    var original_len = input_val.length;
    var caret_pos = input.prop("selectionStart");

    if (input_val.indexOf(",") >= 0) {
        var decimal_pos = input_val.indexOf(",");

        var left_side = input_val.substring(0, decimal_pos);
        var right_side = input_val.substring(decimal_pos);

        left_side = formatNumber(left_side);

        right_side = right_side.replace(",","");

        if (blur === "blur")
            right_side += "00";

        right_side = right_side.substring(0, 5);
        input_val = left_side + "," + right_side;

    } else {
        input_val = formatNumber(input_val);
        input_val = input_val;

        if (blur === "blur") {
            input_val += ",00";
        }
    }

    input.val(input_val);

    var updated_len = input_val.length;
    caret_pos = updated_len - original_len + caret_pos;
    input[0].setSelectionRange(caret_pos, caret_pos);
}