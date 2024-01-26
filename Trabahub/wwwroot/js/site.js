/*Map Functionality Search*/
function handleKeyPress(event) {
    if (event.key === "Enter") {
        MapSearch();
    }
}

async function MapSearch() {
    var GetMapInput = document.getElementById('textmap').value;
    var GetMapSRC = document.getElementById('map');

    var createInputQuery = 'https://maps.google.com/maps?width=100%25&amp;height=600&amp;hl=en&amp;q=(' + GetMapInput + ')&amp;t=&amp;z=14&amp;ie=UTF8&amp;iwloc=B&amp;&output=embed';
    GetMapSRC.src = createInputQuery;

    console.log(createInputQuery);
}
