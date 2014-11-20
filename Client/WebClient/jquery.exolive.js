(function ( $ ) {
	$.ExoLive = function(options) {
	
		function Queue(){
			var queue  = [];
			var offset = 0;
			this.getLength = function(){ return (queue.length - offset); }
			this.isEmpty = function(){ return (queue.length == 0); }
			this.enqueue = function(item){ queue.push(item); }
			this.peek = function(){ return (queue.length > 0 ? queue[offset] : undefined); }			
			this.dequeue = function(){
				if (queue.length == 0) return undefined;
				var item = queue[offset];
				if (++ offset * 2 >= queue.length){
				  queue  = queue.slice(offset);
				  offset = 0;
				}
				return item;
			}
		}
		
        var plugin = this;
		var defaultOptions = {
            apiKey: "",
            onCapsDetecting: function(){},
			onInitComplete: function(){}
        };        
		var settings = $.extend({}, defaultOptions, options );
		var webSessionId = getUniqueCode();
		var runtimeId = getUniqueCode();
		var initState = false;
		var fms = -1;
		
		function getBrowserCaps(){
			var caps = {};
			caps["width"] = screen.width;
			caps["height"] = screen.height;
			caps["availWidth"] = screen.availWidth;
			caps["availHeight"] = screen.availHeight;
			caps["colorDepth"] = screen.colorDepth;
			caps["userAgent"] = navigator.userAgent;
			caps["platform"] = navigator.platform;
			caps["location"] = location.href;
			caps["language"] = navigator.language;
			caps["languages"] = navigator.languages.join();
			caps["GMTOffset"] = (new Date()).getTimezoneOffset();

			var userCaps = settings.onCapsDetecting();
			if(Object.prototype.toString.call(userCaps) == "[object Object]"){
				for (var ucKey in userCaps) {
					caps[ucKey] = userCaps[ucKey];
				}
			}
			
			return caps;			
		}
		
		function createCookie(name, value, days) {
			var expires;
			if (days) {
				var date = new Date();
				date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
				expires = "; expires=" + date.toGMTString();
			} else {
				expires = "";
			}
			document.cookie = escape(name) + "=" + escape(value) + expires + "; path=/";
		}
		
		function getCookie(name) {
			var nameEQ = escape(name) + "=";
			var ca = document.cookie.split(';');
			for (var i = 0; i < ca.length; i++) {
				var c = ca[i];
				while (c.charAt(0) === ' ') c = c.substring(1, c.length);
				if (c.indexOf(nameEQ) === 0) return unescape(c.substring(nameEQ.length, c.length));
			}
			return null;
		}

		function deleteCookie(name) {
			createCookie(name, "", -1);
		}
		
		function escape(value){
			return encodeURIComponent(value);
		}
		function unescape(value){
			return decodeURIComponent(value);
		}	
		
		function getUniqueCode() {
			return Math.round(getRandomArbitrary(100000, 999999)) + "" + (new Date()).getTime();
		}

		function getRandomArbitrary(min, max) {
			return Math.random() * (max - min) + min;
		}
		
		function ensureWebSession() {
			var cookie = getCookie("exolive");
			var result = null;
			if(cookie == null){
				createCookie("exolive", webSessionId, 30);
			}
			return getCookie("exolive");
		}
		
		this.showData = function(){
			alert(settings.apiKey);
		}
		
		
		
		//var initInterval = setInterval(function(){
			var webSession = ensureWebSession();
			var caps = getBrowserCaps();
			$.ajax({
				type: "POST",
				url: "http://localhost:7777/webclient/" + settings.apiKey + "/wsinit",
				data: {
					data: webSession,
					caps: JSON.stringify(caps)
				},
				cache: false,
				success: function (data, textStatus, jqXHR) {
					if(initState) return;
					if(data.ResultCode == 0){
						//clearInterval(initInterval);
						initState = true;
						if(settings.onInitComplete != null){
							settings.onInitComplete();
						}
					}				
				},
				error: function (jqXHR, textStatus, errorThrown) {					
				}
			});
		//}, 1000);
		
		
		return plugin;
    };
}( jQuery ));