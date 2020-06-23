const video = Twilio.Video;

let _videoTrack = null;
let _activeRoom = null;
let _participants = null;

async function getVideoDevices() {
    try {
        let devices = await navigator.mediaDevices.enumerateDevices();
        if (devices && devices.length === 0) {
            await navigator.mediaDevices.getUserMedia({
                video: true
            });
        }

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
}

async function startVideo(deviceId, selector) {
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
}

async function createOrJoinRoom(roomName, token) {
    try {
        if (_activeRoom) {
            _activeRoom.disconnect();
        }

        const audioTrack = await video.createLocalAudioTrack();
        const tracks = [audioTrack, _videoTrack];
        _activeRoom = await video.connect(
            token, {
            name: roomName,
            tracks,
            dominantSpeaker: true
        });

        if (_activeRoom) {
            initialize(_activeRoom.participants);
            _activeRoom
                .on('disconnected',
                    room => room.localParticipant.tracks.forEach(publication => detachTrack(publication.track)))
                .on('participantConnected',
                    participant => this.participants.add(participant))
                .on('participantDisconnected',
                    participant => this.participants.remove(participant))
                .on('dominantSpeakerChanged',
                    dominantSpeaker => this.participants.loudest(dominantSpeaker));
        }
    } catch (error) {
        console.error(`Unable to connect to Room: ${error.message}`);
    }

    return !!_activeRoom;
}

function initialize(participants) {
    _participants = participants;
    if (_participants) {
        _participants.forEach(participant => registerParticipantEvents(participant));
    }
}

function add(participant) {
    if (_participants && participant) {
        _participants.set(participant.sid, participant);
        registerParticipantEvents(participant);
    }
}

function remove(participant) {
    if (_participants && _participants.has(participant.sid)) {
        _participants.delete(participant.sid);
    }
}

function loudest(participant) {
    this.dominantSpeaker = participant;
}

function registerParticipantEvents(participant) {
    if (participant) {
        participant.tracks.forEach(publication => subscribe(publication));
        participant.on('trackPublished', publication => subscribe(publication));
        participant.on('trackUnpublished',
            publication => {
                if (publication && publication.track) {
                    detachRemoteTrack(publication.track);
                }
            });
    }
}

function subscribe(publication) {
    if (isMemberDefined(publication, 'on')) {
        publication.on('subscribed', track => attachTrack(track));
        publication.on('unsubscribed', track => detachTrack(track));
    }
}

function attachTrack(track) {
    if (isMemberDefined(track, 'attach')) {
        const element = track.attach();
        this.renderer.data.id = track.sid;
        this.renderer.appendChild(this.listRef.nativeElement, element);
        //_participantsChanged.emit(true);
    }
}

function detachTrack(track) {
    if (this.isMemberDefined(track, 'detach')) {
        track.detach().forEach(el => el.remove());
        //_participantsChanged.emit(true);
    }
}

function isMemberDefined(instance, member) {
    return !!instance && instance[member] !== undefined;
}

window.videoInterop = {
    getVideoDevices,
    startVideo,
    createOrJoinRoom
};