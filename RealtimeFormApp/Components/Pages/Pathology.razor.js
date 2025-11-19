export async function start(componentInstance) {
    // Populate the UI using the Web Audio API when getUserMedia is called,
    // but don't return the stream here - we need to create a new stream
    // with the right sample rate for the microphone processing
    await navigator.mediaDevices.getUserMedia({ video: false, audio: { sampleRate: 24000 } });

    const micStream = await navigator.mediaDevices.getUserMedia({ audio: { sampleRate: 24000 } });
    processMicrophoneData(micStream, componentInstance);
    return micStream;
}

export function setMute(micStream, mute) {
    micStream.getAudioTracks().forEach(track => track.enabled = !mute);
}

async function processMicrophoneData(micStream, componentInstance) {
    const audioCtx = new AudioContext({ sampleRate: 24000 });
    const source = audioCtx.createMediaStreamSource(micStream);
    const processor = audioCtx.createScriptProcessor(4096, 1, 1);

    source.connect(processor);
    processor.connect(audioCtx.destination);

    processor.onaudioprocess = async function(e) {
        const inputData = e.inputBuffer.getChannelData(0);
        const float32Samples = new Float32Array(inputData);
        const numSamples = float32Samples.length;
        const int16Samples = new Int16Array(numSamples);
        for (let i = 0; i < numSamples; i++) {
            int16Samples[i] = float32Samples[i] * 0x7FFF;
        }
        await componentInstance.invokeMethodAsync('ReceiveAudioDataAsync', new Uint8Array(int16Samples.buffer));
    }

    await componentInstance.invokeMethodAsync('OnMicConnectedAsync');
}
