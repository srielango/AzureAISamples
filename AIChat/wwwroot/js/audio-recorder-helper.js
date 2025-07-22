let audioContext;
let recorder;

async function startRecording() {
    audioContext = new (window.AudioContext || window.webkitAudioContext)();
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    const input = audioContext.createMediaStreamSource(stream);
    recorder = new Recorder(input, { numChannels: 1 });
    recorder.record();
    console.log("Recording started...");
}

function stopRecording(dotNetHelper) {
    recorder.stop();
    recorder.exportWAV(blob => {
        const reader = new FileReader();
        reader.onloadend = () => {
            const base64data = reader.result.split(',')[1];
            dotNetHelper.invokeMethodAsync('OnAudioCaptured', base64data);
        };
        reader.readAsDataURL(blob);
    });
    console.log("Recording stopped.");
}
