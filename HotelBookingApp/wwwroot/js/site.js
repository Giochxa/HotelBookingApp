// Example: Add interactivity to the "View Details" button
document.addEventListener("DOMContentLoaded", function () {
    const viewDetailsButtons = document.querySelectorAll(".btn-primary");

    viewDetailsButtons.forEach(button => {
        button.addEventListener("click", function (event) {
            event.preventDefault();
            alert("You clicked 'View Details' for a hotel!");
        });
    });
});