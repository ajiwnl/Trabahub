/*Map Functionality Search*/
function handleKeyPress(event) {
    if (event.key === "Enter") {
        MapSearch();
    }
}

<<<<<<< HEAD
function MapSearch() {
    var GetMapInput = document.getElementById('textmap').value;
    var GetMapSRC = document.getElementById('map').src;

    var createInputQuery = 'https://maps.google.com/maps?width=100%25&amp;height=600&amp;hl=en&amp;q=(' + GetMapInput + ')&amp;t=&amp;z=14&amp;ie=UTF8&amp;iwloc=B&amp;&output=embed';
    GetMapSRC.src = createInputQuery;
}
=======
// Write your JavaScript code.
>>>>>>> 1418b6560b8c00202594413bd6b8b953e72705b9
