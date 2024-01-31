document.addEventListener('DOMContentLoaded', function () {

    var isUserLogin = document.getElementById('loginlayer') != null;
    var isUserRegister = document.getElementById('registerlayer') != null;

    var navbar = document.getElementById("layoutnav");
    var footer = document.getElementById('layoutfoot');

    if (isUserLogin || isUserRegister) {
        navbar.classList.add('hidenavs');
        footer.classList.add('hidenavs');
    }    
});

