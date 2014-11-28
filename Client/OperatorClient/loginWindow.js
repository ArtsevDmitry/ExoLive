function startAuthentication(login, password) {
    ExoLive.startUserAuthentication(login, password);
}

function onLoginComplete(data){
    if(data === ""){
        var wndLogin = ui().loginWindow;
        var wndMain = ui().mainWindow;
        wndLogin.hide();
        wndMain.show();
    }
}
