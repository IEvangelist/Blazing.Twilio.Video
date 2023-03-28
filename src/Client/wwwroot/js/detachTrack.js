import { isFunctionDefined } from "./isFunctionDefined.js";

export function detachTrack(track) {
    if (isFunctionDefined(track, 'detach')) {
        track.detach()
            .forEach(el => el.remove());
    }
}
