import { waitForElement } from "./waitForElement.js";
import { isPictureInPictureSupported } from "./isPictureInPictureSupported.js";

export async function requestPictureInPicture(selector, onSuccess, onExited) {
    if (!await isPictureInPictureSupported(selector)) {
        console.log(`Picture-in-Picture isn't supported on this browser on element: ${selector}`);
    }

    const videoElement = await waitForElement(selector);
    if (videoElement) {
        videoElement.onenterpictureinpicture = () => {
            if (onSuccess) {
                onSuccess(true);
            }
        };

        videoElement.onleavepictureinpicture = () => {
            if (onExited) {
                onExited();
            }
        };

        try {
            const pipWindow = await videoElement.requestPictureInPicture();
            if (pipWindow) {
                const logWindowDimensions = () => {
                    console.log(`PiP window is ${pipWindow.width}x${pipWindow.height}`);
                };

                logWindowDimensions();
                pipWindow.onresize = logWindowDimensions;

            }
        } catch {
            // Not a big deal... 🙄
            if (onSuccess) {
                onSuccess(false);
            }
        }
    }
}
