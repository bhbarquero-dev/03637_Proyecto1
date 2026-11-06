$.validator.unobtrusive.adapters.addBool('debeserverdadero', 'required');

$.validator.setDefaults({
    highlight: function (elemento, claseError, claseValida) {
        $(elemento).addClass(claseError).removeClass(claseValida).attr('aria-invalid', 'true');
    },
    unhighlight: function (elemento, claseError, claseValida) {
        $(elemento).removeClass(claseError).addClass(claseValida).removeAttr('aria-invalid');
    }
});
