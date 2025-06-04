var login = new Object();
login.memberLogin = document.getElementById("memberLogin");
hostname = window.location.hostname;
login.memberLoginHeading = document.getElementById("memberLoginHeading");
login.login_loader = new Image(32,32);
login.login_loader.src = "http://twimgs.com/gamasutra/images/ajax-login.gif";
login.searchease_login_iframe = document.getElementById("searchease_login_iframe");

login.cookies = unescape(document.cookie).split(";");

login.name = "";
login.email ="";
login.loggedIn = false;
login.image = 'http://twimgs.com/gamasutra/images/questionmark.jpg';

function ajaxRequest(responseHandler) {
	var request;
	try {request = new ActiveXObject('Msxml2.XMLHTTP');}
	catch (e) {
		try {request = new ActiveXObject('Microsoft.XMLHTTP');}
 		catch (e) {
 			try {request = new XMLHttpRequest();}
 			catch (e) {return false;}
		}
 	}
	request.onreadystatechange = function() { 		
		if(request.readyState == 4) {
 			if(request.status  == 200) 
				responseHandler(request.responseText); 
              else 
                 alert("Error code number " + request.status);
         }    	
    } 
	return request;
}

login.loginUser = function(form) {
 	var loginUser;
 	loginUser = ajaxRequest(function(jsonLogin) { 			
 			eval(jsonLogin);
 			if(jsonLogin.login) {
 				login.image = '/blogs/edit/img/portrait/' + jsonLogin.image;
				login.loggedInText(jsonLogin.name);
				login.searchease_login_iframe.src = jsonLogin.searchease_src;				
			}
			else {
				if(jsonLogin.error)
					login.loggedOutText(jsonLogin.error);
			}
		}
	);
	loginUser.open("POST", "/sso/log_in.php?box&nocache=" + new Date().getTime(), true);	
	loginUser.setRequestHeader("Content-Type", "application/x-www-form-urlencoded"); 
	
		var new_email =form.email.value;
		new_email = new_email.replace('+','%2B'); 
		
	if(new_email){
		loginUser.send("email=" + new_email + "&password=" + hex_md5(form.password.value));
	}else{
		loginUser.send("email=" + form.email.value + "&password=" + hex_md5(form.password.value));
	}
	login.loggingIn();
}

login.getCookieValue = function(cookie_name, item) {

	//alert(item)
	
	for(i=0; i < login.cookies.length; i++) {
	//alert("69 "+login.cookies[i])
	//alert("70 "+cookie_name.length)
	//alert("73 "+cookie_name)
		//alert("72"+login.cookies[i].substr(1, cookie_name.length))
		
		if (i != 0){
			var sub = (login.cookies[i]).substr(1, cookie_name.length);
			//login.cookies[i] =login.cookies[i];
		} else {
			// remove first space or whatever before it
			// this is caused from split function when we get cookie document
			//login.cookies[i] =(login.cookies[i]).substr(1);
			var sub = (login.cookies[i]).substr(0, cookie_name.length);
		}
	//	login.cookies[i] =(login.cookies[i]).substr(0);
		
		//alert("cookie is "+cookie_name+" sub is "+sub)
		if(sub == cookie_name) {
			if(item == 0){	
				
				 //alert("76"+login.cookies[i].split("|")[item].substr(cookie_name.length + 1));
				return login.cookies[i].split("|")[item].substr(cookie_name.length + 1);
			} else if(item == 1){			
				//alert(login.cookies[i].split("=")[item])
				return login.cookies[i].split("=")[item];
			} else if(item == 4) {
				return login.cookies[i].split("|")[item];
			} else {
				//alert(login.cookies[i].split("|")[item])
				return login.cookies[i].split("|")[item];
			}	
		}	
	}	
	//return false;
}

login.loggedInText = function(name) {
		
	login.email=login.getCookieValue("GamaDemo200406",4);
	
	if (login.image=='/blogs/edit/img/portrait/false'){
		login.image='http://twimgs.com/gamasutra/images/questionmark.jpg'
	}	
	var name = name.replace("=", "");
        var name = name.replace("+", " ");
	login.memberLoginHeading.innerHTML = '<table cellpadding="3" cellspacing="0" width="100%"><tr><td>' + 
		'<span class="MedGreenHeader">' + name + ' </span></td>' + 
		'<td id="logoutlink" style="text-align: right;">' + 
		'<a href="" onclick="login.logoutUser(); return false;">[LOGOUT]</a>&nbsp;</td></tr></table>';		
	login.memberLogin.innerHTML = '<table cellpadding="0" cellspacing="0"><tr><td style="text-align: center; padding: 5px;"><img src="' + login.image + '" alt="question" /></td>' +
	'<td valign="top" style="padding-top: 2px; padding-bottom: 4px; padding-left: 3px; font-weight: bold;"><a class="loginboxtext" href="/sso/profile.php">My Profile</a><br /><a class="loginboxtext" href="http://'+hostname+'/sso/myblog.php">My Blog</a><br />'
	+'<a class="loginboxtext" href="/sso/profile.php#newsletters">Newsletters</a></td></tr></table>';
        //+'<a class="loginboxtext" target="_blank" href="http://prefctr.inside.gamasutra.com/gamasutra_pc_optin/preferences.aspx?cmd=LoginSignUp&EmailAddr='+login.email+'">Newsletters</a></td></tr></table>';
	if(comment_form = document.getElementById('comment_form')) {
		comment_form.style.display = 'block';
		document.getElementById('no_comment_form').style.display = 'none';
		document.getElementById('user_name_comment').innerHTML = name;
	}	
	login.loggedIn = true;
}

login.loggedOutText = function(error_message) {
	if (!error_message) error_message = '';
	else error_message = '<tr><td colspan="4" style="color: red; text-align: center; padding-bottom: 5px;">' + error_message + '<td></tr>';
	login.memberLoginHeading.innerHTML = '<span class="MedGreenHeader">Member Login <span style="font-size: 9px;"><!--[<a target="_blank" href="http://prefctr.inside.gamasutra.com/gamasutra_pc_optin/preferences.aspx">Newsletters</a>]--></span></span>';
	login.memberLogin.innerHTML =
	  '<form method="POST" action="" onsubmit="return false;" ' + 
	  'name="login_form">' +
	  '<table width="177" border="0" cellspacing="0" cellpadding="0">' +	    
		'<tr><td colspan="4"><img src="http://twimgs.com/gamasutra/images/spacer.gif" border="0" width="5" /></td></tr>' +
		error_message +
		'<tr>' +
		  '<td rowspan="5" width="5"><img src="http://twimgs.com/gamasutra/images/spacer.gif" border="0" width="5" /></td>' +		  
		  '<td width="70" align="right" class="SmWhiteText">Email:&nbsp;&nbsp;</td>' +
		  '<td width="97"><input name="email" type="text" class="login" /></td>' +
		  '<td rowspan="5" width="5"><img src="http://twimgs.com/gamasutra/images/spacer.gif" border="0" width="5" /></td>' +
		'</tr>' +
		'<tr>' +
		  '<td align="right" class="SmWhiteText">Password:&nbsp;&nbsp;</td>' +
		  '<td><input name="password" type="password" class="login" ' +
		  		'onkeypress="if((window.event && window.event.keyCode == 13) || event.keyCode == 13)' + 
		  		'if(login.checkForm(document.login_form)) login.loginUser(document.login_form);" /></td>' +
		'</tr>' +
		'<tr>' +
		  '<td>&nbsp;</td>' +
		  '<td style="padding: 3px;">' +		    
		    '<span class="whiteBorder">' + 		      
		      '<a href=""' + 
                'onclick="if(login.checkForm(document.login_form)) login.loginUser(document.login_form); return false;" class="loginButton">LOGIN</a>' +
		    '</span>' + 
		  '</td>' +
		'</tr>' +
  		'<tr>' +
  		  '<td colspan="2"><a href="/sso/lostpw.php" class="SmWhiteText">Forgot Password?</a>&nbsp;|&nbsp;<a href="https://'+window.location.hostname+'/sso/join.php" class="SmRedText">Sign Up</a></td>' +
  		'</tr>' + 
  		'<tr><td colspan="2"><img src="http://twimgs.com/gamasutra/images/spacer.gif" border="0" width="5" /></td></tr>' +  		
	  '</table>' +
	  '</form>';	
	
	login.loggedIn = false;
	if(comment_form = document.getElementById('comment_form')) {
		comment_form.style.display = 'none';
		document.getElementById('no_comment_form').style.display = 'block';
	}
}

login.loggingIn = function() {	
	login.memberLogin.innerHTML = '<div style="text-align: center; padding-top: 10px; padding-bottom: 10px;"><img src="' + login.login_loader.src + '" /></div>'; 	
} 



login.deleteCookie = function(cookie_name, path) {
	if(!path)
		path = '/';
	var last_month = new Date();
	last_month.setTime(new Date().getTime() - 3600000*24*30);
	document.cookie = cookie_name + '=' + ';expires=' + last_month.toGMTString() + ';path=' + path + ';domain=.gamasutra.com';	
}
login.redirect = function(){
	window.location = "/";
}
login.logoutUser = function() {
	login.memberLogin.innerHTML = '<div style="text-align: center; padding-top: 10px; padding-bottom: 10px;"><img src="' + login.login_loader.src + '" /></div>';
	login.searchease_login_iframe.src = "http://jobs.gamasutra.com/JobSeekerx/JobSeekerLogout.asp";	
	login.deleteCookie("GamaUser");
	login.deleteCookie("GamaDemo2");		
	login.deleteCookie("GamaDemo200406");
	//login.deleteCookie("GamaToken", '/sso/');
	login.deleteCookie("GamaToken");
	
	login.deleteCookie("GamaDemo2");
	login.deleteCookie("myblog_url");	
	setTimeout("login.loggedOutText()", 1000);
	// only for blog edit page to redirect to home page
	login.current_url = location.href;
	if (login.current_url.search(/\/blogs\/edit\//i)!=-1) {
		login.redirect();
		//setTimeout("login.redirect()", 1000);
	}
	
}

/* This is kind of like print_r in PHP */
function printObject(object) {
	var str = '';
	for(prop in object) {
		if(object.hasOwnProperty(prop))
			str += prop + " : " + object[prop] + "\n";
	}
	alert(str);
}

login.checkForm = function(form) {	
	var email = form.email.value;
	var password = form.password.value; 
	var msg ='';
	var reg = /^([A-Za-z0-9_\-\.\+])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]{2,4})$/;
	//email = email.replace('+', '%2B');
	if(email ==''){
		msg = "Please provide your email.\n";
		
	} else if(reg.test(email) == false){
		msg = msg + "Please provide your email.\n";
	}
	if(password == ''){
		msg =msg + "Please provide password."
	}
	if(msg != ''){
		alert(msg);
		return false;
	}
	return true;
}

var image = login.getCookieValue("GamaUser", 3);



login.name = login.getCookieValue("GamaUser",0);

if(login.name && login.getCookieValue("GamaDemo200406",0)) {	
		
		
	if (image == 'false'){
		login.image = 'http://twimgs.com/gamasutra/images/questionmark.jpg';
		
	} else {		
		login.image = '/blogs/edit/img/portrait/' + login.getCookieValue("GamaUser", 3).replace(/\+/, ' ');	
	}
	login.loggedInText(login.name.replace(/\+/g, ' '));	
}
else {	
	login.loggedOutText();		
}