// Pide confirmación antes de un cambio que no se puede deshacer (HU-05).
// Sin JavaScript el botón funciona igual; el servidor sigue validando la transición.
document.addEventListener('click', function (evento) {
    var boton = evento.target.closest('[data-confirmar]');
    if (boton && !window.confirm(boton.getAttribute('data-confirmar'))) {
        evento.preventDefault();
    }
});
