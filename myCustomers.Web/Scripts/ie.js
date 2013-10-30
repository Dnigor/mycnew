$(function() {
  // fix select widths
  $("select")
    .each(function() {
      var el = $(this);
      el.data("origWidth", el.outerWidth());
    })
    .mousedown(function(){
      $(this).css("width", "auto");
    })
    .bind("blur change", function(){
      var el = $(this);
      el.css("width", el.data("origWidth"));
    });

  // fix image radio buttons
  $('li.image-option label').livequery(function() { 
    var id = '#' + $(this).attr('for');
    $(this).children('img').click(function() { 
      $(id).prop('checked', true).click();
    });
  });
});
