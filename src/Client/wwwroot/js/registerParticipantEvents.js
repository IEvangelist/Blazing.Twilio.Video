import { subscribeToPublication } from "./subscribeToPublication.js";

export function registerParticipantEvents(participant, isLocal) {
    if (participant) {
        participant.tracks.forEach(publication => subscribeToPublication(publication, isLocal));
        participant.on('trackPublished', publication => subscribeToPublication(publication, isLocal));
        participant.on('trackUnpublished',
            publication => {
                if (publication && publication.track) {
                    detachRemoteTrack(publication.track);
                }
            });
    }
}
