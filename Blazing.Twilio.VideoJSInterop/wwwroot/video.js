const video = Twilio.Video;

let _token = "";
let _videoTrack = null;

window.videoInterop = {
    getVideoDevices: async () => {
        try {
            await navigator.mediaDevices.getUserMedia({
                audio: true, video: true
            });

            let devices = await navigator.mediaDevices.enumerateDevices();
            if (devices && devices.length) {
                const deviceResults = [];
                devices.filter(device => device.kind === "videoinput")
                    .forEach(device => {
                        const { deviceId, label } = device;
                        deviceResults.push({ deviceId, label });
                    });

                return deviceResults;
            }
        } catch (error) {
            console.log(error);
        }

        return [];
    },
    startVideo: async (deviceId, containerId) => {
        const cameraContainer = document.querySelector(`#${containerId}`);
        if (!cameraContainer) {
            return;
        }

        try {
            if (_videoTrack) {
                _videoTrack.detach().forEach(element => element.remove());
            }

            const tracks = await video.createLocalTracks({ audio: true, video: { deviceId } });
            _videoTrack = tracks.find(t => t.kind === 'video');
            cameraContainer.append(_videoTrack.attach());
        } catch (error) {
            console.log(error);
        }
    }
};
