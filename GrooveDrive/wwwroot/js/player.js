window.GrooveDrive = window.GrooveDrive || {};
GrooveDrive.player = () => document.getElementById('player');
GrooveDrive.streamItem = async function (fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    GrooveDrive.player().src = url;
};
GrooveDrive.play = () => GrooveDrive.player().play();
GrooveDrive.pause = () => GrooveDrive.player().pause();

GrooveDrive.updateProgress = (player) => {
    let progress = (player.currentTime / player.duration) * 100;
    let progressBar = document.querySelector('.progress-bar');
    /*if (video.ended) {
        document.querySelector(".fa-play").style.display = "block"
        document.querySelector(".fa-pause").style.display = "none"
    }*/
    progressBar.style.width = `${progress}%`;
    progressBar.setAttribute('aria-valuenow', progress);
};
