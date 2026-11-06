document.addEventListener('click', function (evento) {
    var boton = evento.target.closest('[data-confirmar]');
    if (boton && !window.confirm(boton.getAttribute('data-confirmar'))) {
        evento.preventDefault();
    }
});
