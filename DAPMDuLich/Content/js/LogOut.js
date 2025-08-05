document.getElementById('logout-btn').addEventListener('click', function (event) {
    event.preventDefault();

    fetch('/Account/Logout', {
        method: 'POST',
        credentials: 'same-origin'
    }).then(response => {
        if (response.ok) {
            window.location.href = '/Account/Login';
        } else {
            alert('Logout failed.');
        }
    }).catch(error => {
        console.error('Error:', error);
    });
});
