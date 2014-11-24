(function ( $ ) {
	$.ExoLive = function(options) {
        var plugin = this;
		var defaultOptions = {
            apiKey: "",
            onCapsDetecting: function(){},
			onInitComplete: function(){},
			onMessageReceive: function(msg){}
        };        
		var settings = $.extend({}, defaultOptions, options );
		var webSessionId = getUniqueCode();
		var runtimeId = getUniqueCode();
		var initState = false;
		var sendState = false;
		var sendInterval;
		var sendArray = [];
		var sendingArray = [];
		var isSendProcess = false;		
		var receiveState = false;
		var receiveInterval;
		var receiveArray = [];
		var lastReceiveMsgNum = 0;
		var isReceiveProcess = false;
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
			return (new Date()).getTime() + "" + Math.round(getRandomArbitrary(100000, 999999));
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
		
		this.debugMessageSend = function(text){
			sendMessage(100, text);
		}
		
		function sendMessage(type, data){
			var msg = {};
			msg["t"] = type;
			msg["id"] = getUniqueCode();
			msg["d"] = data;
			sendArray.push(msg);
		}
		
		function receiveMessage(type, data){
			settings.onMessageReceive(data);
		}
		
		function startSending(){
			sendInterval = setInterval(function(){
				if(!isSendProcess && ((sendArray != null && sendArray.length > 0) || (sendingArray != null && sendingArray.length > 0))){
					isSendProcess = true;
					var webSession = ensureWebSession();
					$.ajax({
						type: "POST",
						url: "http://localhost:7777/webclient/" + settings.apiKey + "/in",
						data: {
							ws: webSession,
							data: JSON.stringify(getSendItems())
						},
						cache: false,
						success: function (data, textStatus, jqXHR) {
							isSendProcess = false;
							if(data.ResultCode == 0){
								sendingArray = [];								
							}							
						},
						error: function (jqXHR, textStatus, errorThrown) {
							isSendProcess = false;
						}
					});
				}
			}, 200);
			sendState = true;
		}
		
		function getSendItems(){
			if(sendingArray != null && sendingArray.length > 0)
				return sendingArray;
				
			while(sendArray.length){
				sendingArray.push(sendArray.pop());
			}
			return sendingArray;
		}
		
		function stopSending(){
			clearInterval(sendInterval);
			sendState = false;
		}
		
		function startReceiving(){
			receiveInterval = setInterval(function(){
				if(!isReceiveProcess){
					isReceiveProcess = true;
					var webSession = ensureWebSession();
					$.ajax({
						type: "POST",
						url: "http://localhost:7777/webclient/" + settings.apiKey + "/out",
						timeout: 7000,
						data: {
							ws: webSession,
							lid: lastReceiveMsgNum
						},
						cache: false,
						success: function (data, textStatus, jqXHR) {							
							try{
								if(Object.prototype.toString.call(data) == "[object Array]"){
									var curMsgNum = lastReceiveMsgNum;
									
									for(var i=0; i<data.length; i++) if(parseInt(data[i].n) > lastReceiveMsgNum) lastReceiveMsgNum = parseInt(data[i].n);
									for(var i=0; i<data.length; i++) {
										if(parseInt(data[i].n) > curMsgNum)
											receiveMessage(data[i].t, data[i].d);
									}
								}
							} finally{
								isReceiveProcess = false;
							}
						},
						error: function (jqXHR, textStatus, errorThrown) {
							isReceiveProcess = false;
						}
					});
				}
			}, 200);
			receiveState = true;
		}
		
		function stopReceiving(){
			clearInterval(receiveInterval);
			receiveState = false;
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
		
		// sendMessage(100, "123");
		// sendMessage(100, "uwioh rweughfeurhg uioehopirgh eoihjoi");
		
		startSending();
		startReceiving();
		
		return plugin;
    };
}( jQuery ));