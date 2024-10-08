﻿// Hide navigation bar on login and register
document.addEventListener('DOMContentLoaded', function () {

    OnLoginRegisterLayers();
    OnOwnerRegister();
});

function OnLoginRegisterLayers() {
    //Check If User Is On Login/Register Page
    var isUserLogin = document.getElementById('loginlayer') != null;
    var isUserRegister = document.getElementById('registerlayer') != null;

    //Get Nav and Footer Elements/Contents
    var navbar = document.getElementById("layoutnav");
    var footer = document.getElementById('layoutfoot');

    //If True, Hide Nav/Footer Bars
    if (isUserLogin || isUserRegister) {
        navbar.classList.add('hidenavs');
        footer.classList.add('hidenavs');
    }
}

function OnOwnerRegister() {
    //Check If User Is On Owner Registration Page
    var isOwnerRegister = document.getElementById('listinglayer') != null;
    var isForgotPassword = document.getElementById('forgotlayer') != null;
    var navcontents = document.getElementById('navcontents');

    //If True, Hide Nav Contents and Disable Redirect Action
    if (isOwnerRegister || isForgotPassword) {
        navcontents.classList.add('hidenavs');
    }
}

//Functionalities on Listing Page
function handleTabClick(clickedTabId) {
    var tabs = ['basic', 'location', 'facilities', 'listdetails', 'listphotos', 'listpublish'];

    tabs.forEach(function (tabId) {
        var tab = document.getElementById(tabId);
        if (tabId === clickedTabId) {
            tab.classList.remove('hidelcontent');
        } else {
            tab.classList.add('hidelcontent');
        }
    });
}

document.getElementById('basicTab').addEventListener('click', function () {
    handleTabClick('basic');
});

document.getElementById('locationTab').addEventListener('click', function () {
    handleTabClick('location');
});

document.getElementById('facilitiesTab').addEventListener('click', function () {
    handleTabClick('facilities');
});

document.getElementById('detailsTab').addEventListener('click', function () {
    handleTabClick('listdetails');
});

document.getElementById('photosTab').addEventListener('click', function () {
    handleTabClick('listphotos');
});

document.getElementById('publishTab').addEventListener('click', function () {
    handleTabClick('listpublish');
});


//Contact Page Validation
function FeedbackValidation() {
    var name = document.getElementsByName("name")[0].value;
    var email = document.getElementsByName("email")[0].value;
    var contact = document.getElementsByName("contact")[0].value;
    var message = document.getElementsByName("message")[0].value;

    if (name === "" || email === "" || contact === "") {
        alert("Name, Email, and Contact No are required fields");
        return false;
    }

    // Validate Contact No starts with 09 and is followed by 9 more numeric digits
    if (!/^09\d{9}$/.test(contact)) {
        alert("Please enter a valid Philippine mobile number starting with 09");
        return false;
    }

    // Validate Name has between 20 and 55 characters
    if (name.length < 20 || name.length > 55) {
        alert("Name must be between 20 and 55 characters");
        return false;
    }

    // Validate Message has at least 20 characters
    if (message.length < 20) {
        alert("Message must contain at least 20 characters");
        return false;
    }

    // Additional validation logic can be added here if needed

    return true; // Form will be submitted if all validations pass
}

//Confirm Password Validation
function checkPasswordMatch() {
    var password = document.getElementById("passwordreg").value;
    var confirmPassword = document.getElementById("confirmPassword").value;
    var errorSpan = document.getElementById("passwordMatchError");
    var registerButton = document.getElementById("registerButton");

    if (password !== confirmPassword) {
        errorSpan.innerHTML = "Passwords do not match";
        registerButton.disabled = true;
    } else {
        errorSpan.innerHTML = "";
        registerButton.disabled = false;
    }
}

var emailChecked = false;

// Email validation
function handleEnter(event) {
    if (event.key === 'Enter') {
        checkEmail();
    }
}

function checkEmail() {
    var email = document.getElementById('floatingInputEmail').value;

    $.ajax({
        url: '/Credentials/CheckEmail',
        type: 'POST',
        data: { email: email },
        success: function (data) {
            if (data.exists) {
                // Email exists, enable the password field
                document.getElementById('floatingInputPassword').disabled = false;
                document.getElementById('confirmPassword').disabled = false;
                document.getElementById('emailValidationMessage').innerText = 'Email found. You can proceed.';
                emailChecked = true; // Set the flag to true
            } else {
                // Email does not exist, show an error message and keep the password field disabled
                document.getElementById('emailValidationMessage').innerText = 'Email not found. Please check and try again.';
            }
        },
        error: function () {
            alert('Error checking email.');
        }
    });
}

// Update the form submission function
function submitForm() {
    if (!emailChecked) {
        return false; // Prevent the form from submitting
    }
    // Continue with form submission
    document.forms[0].submit();
}


function checkForgotPasswordMatch() {
    var password = document.getElementById('floatingInputPassword').value;
    var confirmPassword = document.getElementById('confirmPassword').value;
    var errorElement = document.getElementById('passwordMatchError');

    if (password !== confirmPassword) {
        errorElement.innerText = 'Passwords do not match';
    } else {
        errorElement.innerText = '';
    }
}

