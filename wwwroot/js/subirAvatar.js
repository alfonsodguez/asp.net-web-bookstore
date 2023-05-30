$('#selectorImagen').change(function (ev) {
    const ficheroSeleccionado = ev.target.files[0]
    const reader = new FileReader()

    reader.addEventListener("load", function (evt) {
        $('#imagenUsuario').attr("src", evt.target.result)
        $('#botonUploadImagen').removeAttr("disabled")
        $('#botonUploadImagen').click(function (evtb) {
            $(evtb.target).attr('disabled', '')
            var datos = new FormData()
            datos.append("imagen", ficheroSeleccionado)

            $.ajax({
                method: 'POST',
                url: 'https://localhost:44311/api/RESTAgapea/uploadImagen',
                data: datos,
                contentType: false, //<---ojo si pones 'multipart/form-data' no define el boundary, y multer kaska...
                processData: false
            })
                .done(function (respServer) {
                    console.log('la respuesta del servidor es...', respServer)
                    $('#respuestaServer').append('<p class="text-danger">' + respServer.mensaje + '</p>')
                })
                .fail(function (error) {
                    console.log('error en peticion ajax...', error)
                })
                .always(function () {
                    $('#botonUploadImagen').removeAttr('disabled')
                })
        })
    })
    reader.readAsDataURL(ficheroSeleccionado)
})