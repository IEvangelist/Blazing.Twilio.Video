let _videoTrack = null;
let _activeRoom = null;
let _participants = new Map();
let _dominantSpeaker = null;

async function getVideoDevices() {
    try {
        let devices = await navigator.mediaDevices.enumerateDevices();
        if (devices &&
           (devices.length === 0 || devices.every(d => d.deviceId === ""))) {
            await navigator.mediaDevices.getUserMedia({
                video: true
            });
        }

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

        _videoTrack = await Twilio.Video.createLocalVideoTrack({ deviceId });
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

        const audioTrack = await Twilio.Video.createLocalAudioTrack();
        const tracks = [audioTrack, _videoTrack];
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
    _dominantSpeaker = participant;
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
        const audioOrVideo = track.attach();
        audioOrVideo.id = track.sid;

        if ('video' === audioOrVideo.tagName.toLowerCase()) {
            const responsiveDiv = document.createElement('div');
            responsiveDiv.id = track.sid;
            responsiveDiv.classList.add('embed-responsive');
            responsiveDiv.classList.add('embed-responsive-16by9');

            const responsiveItem = document.createElement('div');
            responsiveItem.classList.add('embed-responsive-item');

            // Similar to.
            // <div class="embed-responsive embed-responsive-16by9">
            //   <div id="camera" class="embed-responsive-item">
            //     <video></video>
            //   </div>
            // </div>
            responsiveItem.appendChild(audioOrVideo);
            responsiveDiv.appendChild(responsiveItem);
            document.getElementById('participants').appendChild(responsiveDiv);
        } else {
            document.getElementById('participants')
                .appendChild(audioOrVideo);
        }
    }
}

function detachTrack(track) {
    if (this.isMemberDefined(track, 'detach')) {
        track.detach()
            .forEach(el => {
                if ('video' === el.tagName.toLowerCase()) {
                    const parent = el.parentElement;
                    if (parent && parent.id !== 'camera') {
                        const grandParent = parent.parentElement;
                        if (grandParent) {
                            grandParent.remove();
                        }
                    }
                } else {
                    el.remove()
                }
            });
    }
}

function isMemberDefined(instance, member) {
    return !!instance && instance[member] !== undefined;
}

async function leaveRoom() {
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
}

window.videoInterop = {
    getVideoDevices,
    startVideo,
    createOrJoinRoom,
    leaveRoom
};

window.store = {
    get: key => window.localStorage[key],
    set: (key, value) => window.localStorage[key] = value,
    delete: key => delete window.localStorage[key]
};