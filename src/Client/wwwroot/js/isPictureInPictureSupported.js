import { waitForElement } from "./waitForElement.js";

export async function isPictureInPictureSupported(selector) {
    const videoElement = await waitForElement(selector);
    if (!videoElement) {
        const errorMessage = `Unable to get HTML video element matching ${selector}`;
        console.log(errorMessage);
        return false;
    }

    return document.pictureInPictureEnabled
        && videoElement.disablePictureInPicture === false;
}
