(function ( $ ) {
	$.ExoLive = function(options) {
        var plugin = this;
		var defaultOptions = {
            apiKey: "",
            onCapsDetecting: function(){}
        };        
		var settings = $.extend({}, defaultOptions, options );
		
		function getKVObj(k, v){
			return { k: k, v: v };
		}
		function getBrowserCaps(){
			var caps = [];
			caps.push(getKVObj("width", screen.width));
			caps.push(getKVObj("height", screen.height));
			caps.push(getKVObj("availWidth", screen.availWidth));
			caps.push(getKVObj("availHeight", screen.availHeight));
			caps.push(getKVObj("colorDepth", screen.colorDepth));
			caps.push(getKVObj("userAgent ", navigator.userAgent));
			caps.push(getKVObj("platform", navigator.platform));
			caps.push(getKVObj("location", location.href));			
			return caps;			
		}
		
		this.showData = function(){
			alert(settings.apiKey);
		}
		
		setTimeout(function(){ 
			var caps = getBrowserCaps();
			$.ajax({
				type: "POST",
				url: 'http://localhost:7777/webclient/662F996C0F4A4B2F9DCB9E269463CEF10/test',
				data: {
					apiKey: "1234567890"
				},
				cache: false,
				success: function (data, textStatus, jqXHR) {
					alert("Success " + data);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					alert("Error " + errorThrown);
				}
			});
		}, 1000);
		
		return plugin;
    };
}( jQuery ));