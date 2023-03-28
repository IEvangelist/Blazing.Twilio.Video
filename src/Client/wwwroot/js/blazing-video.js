import { requestPictureInPicture } from "./requestPictureInPicture.js"
import { exitPictureInPicture } from "./exitPictureInPicture.js";
import { waitForElement } from "./waitForElement.js";
import { registerParticipantEvents } from "./registerParticipantEvents.js";
import { detachTrack } from "./detachTrack.js";

let _videoTrack = null;
let _audioTrack = null;
let _activeRoom = null;
let _participants = new Map();
let _isAttemptingToStartVideo = false;
let _dominantSpeaker = null;

/*
    Requests the navigator.mediaDevices for video input.
    @returns {Promise<string>}.
    global navigator,console
*/
export async function requestVideoDevices() {
    try {
        // Ask the first time to prime the underlying device list.
        let devices = await navigator.mediaDevices.enumerateDevices();
        if (devices &&
            (devices.length === 0 || devices.every(d => d.deviceId === ""))) {
            await navigator.mediaDevices.getUserMedia({
                video: true
            });
        }

        // Ask again, if the user allowed it the device list is now populated.
        // If not, the const returns an empty array.
        devices = await navigator.mediaDevices.enumerateDevices();
        if (devices && devices.length) {
            const deviceResults = [];
            devices.filter(device => device.kind === 'videoinput')
                .forEach(device => {
                    const { deviceId, label } = device;
                    deviceResults.push({ deviceId, label });
                });

            return JSON.stringify(deviceResults);
        }
    } catch (error) {
        console.log(error);
    }

    return JSON.stringify([]);
}

export async function startVideo(deviceId, selector) {
    if (_isAttemptingToStartVideo === true) {
        return;
    }

    _isAttemptingToStartVideo = true;

    const cameraContainer = await waitForElement(selector);
    if (!cameraContainer) {
        const errorMessage = `Unable to get HTML element matching ${selector}`;
        console.log(errorMessage);
        return _isAttemptingToStartVideo = false;
    }

    try {
        if (_videoTrack) {
            _videoTrack.detach().forEach(child => child.remove());
            _videoTrack.stop();
            _videoTrack = null;
        }

        _videoTrack = await Twilio.Video.createLocalVideoTrack({ deviceId });
        const videoElement = _videoTrack.attach();
        cameraContainer.append(videoElement);
    } catch (error) {
        console.log(error);
        return _isAttemptingToStartVideo = false;
    } finally {
        _isAttemptingToStartVideo = false;
    }

    return true;
}

export function stopVideo() {
    try {
        if (_videoTrack) {
            _videoTrack.detach().forEach(child => child.remove());
            _videoTrack.stop();
            _videoTrack = null;
        }
    } catch (error) {
        console.log(error);
    }
}

export async function createOrJoinRoom(roomName, token) {
    try {
        if (_activeRoom) {
            _activeRoom.disconnect();
        }

        if (!_videoTrack) {
            await startVideo(localStorage['camera-device-id'], 'participant-1');
        }

        if (_audioTrack) {
            _audioTrack.detach().forEach(child => child.remove());
            _audioTrack.stop();
            _audioTrack = null;
        }

        _audioTrack = await Twilio.Video.createLocalAudioTrack();
        const tracks = [_audioTrack, _videoTrack];
        _activeRoom = await Twilio.Video.connect(
            token, {
            name: roomName,
            tracks,
            dominantSpeaker: true
        });

        if (_activeRoom) {
            initialize(_activeRoom.participants);
            _activeRoom
                .on('disconnected',
                    room => room.localParticipant.tracks.forEach(
                        publication => detachTrack(publication.track)))
                .on('participantConnected', participant => add(participant))
                .on('participantDisconnected', participant => remove(participant))
                .on('dominantSpeakerChanged', dominantSpeaker => loudest(dominantSpeaker));
        }
    } catch (error) {
        const message = JSON.stringify(error, Object.getOwnPropertyNames(error));
        console.error(`Unable to connect to Room: ${message}`);
        return false;
    }

    return !!_activeRoom;
}

const initialize = (participants) => {
    _participants = participants;
    if (_participants) {
        _participants.forEach(participant => registerParticipantEvents(participant, true));
    }
};

const add = (participant) => {
    if (_participants && participant) {
        _participants.set(participant.sid, participant);
        registerParticipantEvents(participant, false);
    }
};

const remove = (participant) => {
    if (_participants && _participants.has(participant.sid)) {
        _participants.delete(participant.sid);
    }
};

const loudest = (participant) => {
    _dominantSpeaker = participant;
};

export function leaveRoom() {
    try {
        if (_activeRoom) {
            _activeRoom.disconnect();
            _activeRoom = null;
        }

        if (_participants) {
            _participants.clear();
        }
    }
    catch (error) {
        console.error(error);
        return false;
    }

    return true;
}

export {
    requestPictureInPicture,
    exitPictureInPicture
};