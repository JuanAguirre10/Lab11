document.addEventListener('DOMContentLoaded', function () {
    var modal = document.getElementById('modalCancelar');
    if (modal) {
        modal.addEventListener('show.bs.modal', function (evento) {
            var boton = evento.relatedTarget;
            modal.querySelector('form').setAttribute('action', boton.getAttribute('data-url'));
            modal.querySelector('[data-detalle]').textContent = boton.getAttribute('data-detalle');
        });
    }
});
