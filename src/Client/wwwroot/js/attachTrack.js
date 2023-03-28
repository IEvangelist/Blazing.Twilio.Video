import { isFunctionDefined } from "./isFunctionDefined.js";

export function attachTrack(track, isLocal) {
    if (isFunctionDefined(track, 'attach')) {
        const audioOrVideo = track.attach();
        audioOrVideo.id = track.sid;

        document.getElementById(`participant-${isLocal ? '1' : '2'}`)
            .appendChild(audioOrVideo);
    }
}
