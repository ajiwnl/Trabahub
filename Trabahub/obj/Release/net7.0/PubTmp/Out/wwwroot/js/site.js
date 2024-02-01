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

