document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('browse').addEventListener('click', () => {
        document.body.classList.add('form-open');
        document.getElementById('overlay').style.display = 'block';
        document.getElementById('form').style.display = 'flex';
    });
    document.getElementById('overlay').addEventListener('click', closeForm);
    document.getElementById('cancel').addEventListener('click', closeForm);
    document.getElementById('confirm').addEventListener('click', () => {
        let path = document.getElementById('path-input').value;
        document.getElementById('path').innerHTML = path;

        closeForm();
    });
});

function closeForm() {
    document.body.classList.remove('form-open');
    document.getElementById('overlay').style.display = 'none';
    document.getElementById('form').style.display = 'none';
}