// TODO
$('a[id^=["btnEditarDireccion_"]').click((ev) => {
    var idDireccion = $(ev.target).attr('id').split('_')[1];
    $.ajax({
        method: 'POST',
        url: 'https://localhost:44311/api/RESTAgapea/DameDireccion',
        data: { calle: , cp: }
    })
});