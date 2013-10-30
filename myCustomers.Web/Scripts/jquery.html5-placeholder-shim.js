(function($) {
  // @todo Document this.
  $.extend($,{ placeholder: {
      browser_supported: function() {
        return this._supported !== undefined ?
          this._supported :
          ( this._supported = !!('placeholder' in $('<input type="text">')[0]) );
      },
      shim: function(opts) {
        var config = {
          color: '#888',
          cls: 'placeholder',
          selector: 'input[placeholder], textarea[placeholder]'
        };
        $.extend(config,opts);
        return !this.browser_supported() && $(config.selector).livequery(function() { $(this)._placeholder_shim(config); });
      }
  }});

  $.extend($.fn,{
    _placeholder_shim: function(config) {

      return this.each(function() {
        var $this = $(this);

          if( $this.data('placeholder') ) {
            return true;
          }

          var possible_line_height = {};
          if( !$this.is('textarea') && $this.css('height') != 'auto') {
            possible_line_height = { lineHeight: $this.css('height'), whiteSpace: 'nowrap' };
          }
          
          var hadFocus = $this.is(":focus");

          $container = $this.wrap('<div style="display:inline-block;position:relative" />').parent();

          if (hadFocus) {
              // IE needs this delay
              setTimeout(function () {
                $this.focus();
                $this.val($this.val());
              }, 10);
            }

          var $label = $('<label />')
            .text($this.attr('placeholder'))
            .addClass(config.cls)
            .css($.extend({
              position:'absolute',
              display: 'inline-block',
              float: 'none',
              overflow: 'hidden',
              textAlign: 'left',
              color: config.color,
              cursor: 'text',
              paddingTop: $this.css('padding-top'),
              paddingRight: $this.css('padding-right'),
              paddingBottom: $this.css('padding-bottom'),
              paddingLeft: $this.css('padding-left'),
              fontSize: $this.css('font-size'),
              fontFamily: $this.css('font-family'),
              //fontStyle: $this.css('font-style'),
              fontWeight: $this.css('font-weight'),
              textTransform: $this.css('text-transform'),
              backgroundColor: 'transparent',
              width: $this.width(),
              top: 0,
              left: 0
            }, possible_line_height))
            .attr('for', this.id)
            .data('target', $this)
            .click(function(){ $(this).data('target').focus(); })
            .appendTo($container);
        
          function changePlaceholderVisibility() {
            $label[$this.val().length ? 'hide' : 'show']();
          };

          $this
            .resize(function() { $label.width($this.width()); })
            .data('placeholder', $label)
            .focus(function() { $label.hide(); })
            .change(changePlaceholderVisibility)
            .blur(changePlaceholderVisibility);

          changePlaceholderVisibility();
      });
    }
  });
})(jQuery);

jQuery(document).add(window).bind('ready load', function() {
  if (jQuery.placeholder) {
    jQuery.placeholder.shim();
  }
});

