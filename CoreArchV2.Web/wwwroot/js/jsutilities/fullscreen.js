$("#panel-fullscreen").click(function (e) {
    e.preventDefault();
    var $this = $(this);
    if ($this.children('i').hasClass('glyphicon-resize-full')) {
        $this.children('i').removeClass('glyphicon-resize-full');
        $this.children('i').addClass('glyphicon-resize-small');
        $('.table-responsive').css("height", "100vh");
    }
    else if ($this.children('i').hasClass('glyphicon-resize-small')) {
        $this.children('i').removeClass('glyphicon-resize-small');
        $this.children('i').addClass('glyphicon-resize-full');
        $('.table-responsive').css("height", "50vh");
    }
    $(this).closest('.panel').toggleClass('panel-fullscreen');
});
$("#panel-fullscreen2").click(function (e) {
    e.preventDefault();
    var $this = $(this);
    if ($this.children('i').hasClass('glyphicon-resize-full')) {
        $this.children('i').removeClass('glyphicon-resize-full');
        $this.children('i').addClass('glyphicon-resize-small');
        $('.table-responsive').css("height", "100vh");
    }
    else if ($this.children('i').hasClass('glyphicon-resize-small')) {
        $this.children('i').removeClass('glyphicon-resize-small');
        $this.children('i').addClass('glyphicon-resize-full');
        $('.table-responsive').css("height", "50vh");
    }
    $(this).closest('.panel').toggleClass('panel-fullscreen');
});
$("#panel-fullscreen3").click(function (e) {
    e.preventDefault();
    var $this = $(this);
    if ($this.children('i').hasClass('glyphicon-resize-full')) {
        $this.children('i').removeClass('glyphicon-resize-full');
        $this.children('i').addClass('glyphicon-resize-small');
        $('.table-responsive').css("height", "100vh");
    }
    else if ($this.children('i').hasClass('glyphicon-resize-small')) {
        $this.children('i').removeClass('glyphicon-resize-small');
        $this.children('i').addClass('glyphicon-resize-full');
        $('.table-responsive').css("height", "50vh");
    }
    $(this).closest('.panel').toggleClass('panel-fullscreen');
});
$("#panel-fullscreen4").click(function (e) {
    e.preventDefault();
    var $this = $(this);
    if ($this.children('i').hasClass('glyphicon-resize-full')) {
        $this.children('i').removeClass('glyphicon-resize-full');
        $this.children('i').addClass('glyphicon-resize-small');
        $('.table-responsive').css("height", "100vh");
    }
    else if ($this.children('i').hasClass('glyphicon-resize-small')) {
        $this.children('i').removeClass('glyphicon-resize-small');
        $this.children('i').addClass('glyphicon-resize-full');
        $('.table-responsive').css("height", "50vh");
    }
    $(this).closest('.panel').toggleClass('panel-fullscreen');
});