let _videoTrack = null;
let _audioTrack = null;
let _activeRoom = null;
let _participants = new Map();
let _dominantSpeaker = null;

export const getVideoDevices = async () => {
    try {
        // Ask the first time to prime the underlying device list.
        let devices = await navigator.mediaDevices.enumerateDevices();
        if (devices &&
            (devices.length === 0 || devices.every(d => d.deviceId === ""))) {
            await navigator.mediaDevices.getUserMedia({
                video: true
            });
        }

        // Ask again, if the user allowed it the device list is now poplated.
        // If not, the const returns an empty array.
        devices = await navigator.mediaDevices.enumerateDevices();
        if (devices && devices.length) {
            const deviceResults = [];
            devices.filter(device => device.kind === 'videoinput')
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
};

const waitForElement = (selector) => {
    return new Promise(resolve => {
        if (document.querySelector(selector)) {
            return resolve(document.querySelector(selector));
        }

        const observer = new MutationObserver(mutations => {
            if (document.querySelector(selector)) {
                resolve(document.querySelector(selector));
                observer.disconnect();
            }
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    });
};

export const startVideo = async (deviceId, selector) => {
    const cameraContainer = await waitForElement(selector);
    if (!cameraContainer) {
        return false;
    }

    try {
        resetExistingTrack({
            track: _videoTrack,
            nullOut: () => _videoTrack = null
        });

        _videoTrack = await Twilio.Video.createLocalVideoTrack({ deviceId });
        const videoEl = _videoTrack.attach();
        cameraContainer.append(videoEl);
    } catch (error) {
        console.log(error);
        return false;
    }

    return true;
};

export const stopVideo = () => {
    try {
        resetExistingTrack({
            track: _videoTrack,
            nullOut: () => _videoTrack = null
        });
    } catch (error) {
        console.log(error);
    }
};

export const createOrJoinRoom = async (roomName, token) => {
    try {
        if (_activeRoom) {
            _activeRoom.disconnect();
        }

        if (!_videoTrack) {
            await startVideo(localStorage['camera-device-id'], 'participant-1');
        }

        resetExistingTrack({
            track: _audioTrack,
            nullOut: () => _audioTrack = null
        });

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
        console.error(`Unable to connect to Room: ${error.message}`);
        return false;
    }

    return !!_activeRoom;
};

const resetExistingTrack = ({
    track = null,
    nullOut = () => track = null
}) => {
    if (track) {
        track.detach().forEach(child => child.remove());
        track.stop();
        
        if (nullOut) {
            nullOut();
        }
    }
};

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

const registerParticipantEvents = (participant, isLocal) => {
    if (participant) {
        participant.tracks.forEach(publication => subscribe(publication, isLocal));
        participant.on('trackPublished', publication => subscribe(publication, isLocal));
        participant.on('trackUnpublished',
            publication => {
                if (publication && publication.track) {
                    detachRemoteTrack(publication.track);
                }
            });
    }
};

const subscribe = (publication, isLocal) => {
    if (isMemberDefined(publication, 'on')) {
        publication.on('subscribed', track => attachTrack(track, isLocal));
        publication.on('unsubscribed', track => detachTrack(track));
    }
};

const attachTrack = (track, isLocal) => {
    if (isMemberDefined(track, 'attach')) {
        const audioOrVideo = track.attach();
        audioOrVideo.id = track.sid;

        document.getElementById(`participant-${isLocal ? '1' : '2'}`)
            .appendChild(audioOrVideo);
    }
};

const detachTrack = (track) => {
    if (this.isMemberDefined(track, 'detach')) {
        track.detach()
            .forEach(el => el.remove());
    }
};

const isMemberDefined = (instance, member) => {
    return !!instance && instance[member] !== undefined;
};

export const leaveRoom = async () => {
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
    }
};