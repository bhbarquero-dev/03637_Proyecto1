// Ajustes de la validación en el cliente. Se carga después de jquery.validate.unobtrusive
// y antes de que este analice la página.

// La casilla obligatoria debe estar marcada (ver DebeSerVerdaderoAttribute).
$.validator.unobtrusive.adapters.addBool('debeserverdadero', 'required');

// Anuncia a los lectores de pantalla qué campos tienen error (WCAG 3.3.1).
$.validator.setDefaults({
    highlight: function (elemento, claseError, claseValida) {
        $(elemento).addClass(claseError).removeClass(claseValida).attr('aria-invalid', 'true');
    },
    unhighlight: function (elemento, claseError, claseValida) {
        $(elemento).removeClass(claseError).addClass(claseValida).removeAttr('aria-invalid');
    }
});
