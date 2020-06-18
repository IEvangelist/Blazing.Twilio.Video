const video = Twilio.Video;

let _authToken = "";
let _videoTrack = null;
let _activeRoom = null;
let _participants = null;

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
    startVideo: async (deviceId, selector) => {
        const cameraContainer = document.querySelector(selector);
        if (!cameraContainer) {
            return;
        }

        try {
            if (_videoTrack) {
                _videoTrack.detach().forEach(element => element.remove());
            }

            _videoTrack = await video.createLocalVideoTrack({ deviceId });
            const videoEl = _videoTrack.attach();
            cameraContainer.append(videoEl);
        } catch (error) {
            console.log(error);
        }
    },
    getAuthToken: async () => {
        const response = await fetch("api/twilio/token");
        if (response.ok) {
            const json = await response.json();
            return json.token;
        }
        return null;
    },
    createOrJoinRoom: async (roomName) => {
        try {
            if (_activeRoom) {
                _activeRoom.disconnect();
            }

            const token = await videoInterop.getAuthToken();
            const audioTrack = await video.createLocalAudioTrack();
            const tracks = [audioTrack, _videoTrack];
            _activeRoom = await video.connect(
                token, {
                roomName,
                tracks,
                dominantSpeaker: true
            });
        } catch (error) {
            console.error(`Unable to connect to Room: ${error.message}`);
        }

        return !!_activeRoom;
    }
};