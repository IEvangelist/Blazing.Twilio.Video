import { isFunctionDefined } from "./isFunctionDefined.js";
import { detachTrack } from "./detachTrack.js";
import { attachTrack } from "./attachTrack.js";

export function subscribeToPublication(publication, isLocal) {
    if (isFunctionDefined(publication, 'on')) {
        publication.on('subscribed', track => attachTrack(track, isLocal));
        publication.on('unsubscribed', track => detachTrack(track));
    }
}
