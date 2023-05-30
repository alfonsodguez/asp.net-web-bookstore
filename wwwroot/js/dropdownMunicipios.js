$('#inputProvincia').change(function (evt) {
    $('#inputLocalidad > option[value != 0]').remove();

    const codProv = evt.target.value;
    if (codProv != "0") {
        $('#inputLocalidad').removeAttr('disabled');
        // ajax 
        const url = 'https://localhost:44311/api/RESTAgapea/DevolverMunicipios/?codpro=' + codProv
        $.get(url)
            .done((muncipios) => {
                muncipios.forEach((municipio) => {
                    $('#inputLocalidad').append('<option value=' + municipio.codMun + '>' + municipio.nombreMunicipio + '</option>');
                });
            })
            .fail((err) => { console.log(err) });
    }
})