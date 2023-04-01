function worktime() {
    var startworkval = document.getElementById('startwork') || document.getElementById('gym_startwork') ;
    var endworkval = document.getElementById('endwork') || document.getElementById('gym_endwork');
        if (Number(startworkval.value) > Number(endworkval.value)) {
            alert('Start Work cannot be greater than End Work');
            event.preventDefault();
        }
}

