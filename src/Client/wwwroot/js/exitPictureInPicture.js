export async function exitPictureInPicture(onExited) {
    let exited = false;
    try {
        if (document.pictureInPictureElement) {
            await document.exitPictureInPicture();
            exited = true;
        }
    } finally {
        if (onExited) {
            onExited(exited);
        }
    }
}
