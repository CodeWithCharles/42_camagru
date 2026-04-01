export function createCaptureController({ stage, video, image, emptyState, startButton, stopButton }) {
  let stream = null;
  let sourceType = "empty";
  let uploadedName = null;

  async function startCamera() {
    if (!navigator.mediaDevices?.getUserMedia) {
      throw new Error("Webcam preview is unavailable in this browser context.");
    }

    stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: false });
    video.srcObject = stream;
    await video.play();
    sourceType = "camera";
    uploadedName = null;
    render();
  }

  function stopCamera() {
    stream?.getTracks().forEach((track) => track.stop());
    stream = null;
    if (sourceType === "camera") {
      sourceType = "empty";
    }
    render();
  }

  async function loadUpload(file) {
    uploadedName = file.name;
    sourceType = "upload";
    image.src = await readFileAsDataUrl(file);
    await image.decode();
    stopCamera();
    sourceType = "upload";
    render();
  }

  function hasBaseSource() {
    return sourceType === "camera" ? Boolean(stream) : sourceType === "upload" && Boolean(image.src);
  }

  async function exportPreview({ overlays, surface }) {
    if (!hasBaseSource()) {
      throw new Error("Select a webcam or uploaded image first.");
    }

    const canvas = document.createElement("canvas");
    const context = canvas.getContext("2d");
    if (!context) {
      throw new Error("Canvas export is unavailable.");
    }

    if (sourceType === "camera") {
      canvas.width = video.videoWidth;
      canvas.height = video.videoHeight;
      context.drawImage(video, 0, 0, canvas.width, canvas.height);
    } else {
      canvas.width = image.naturalWidth;
      canvas.height = image.naturalHeight;
      context.drawImage(image, 0, 0, canvas.width, canvas.height);
    }

    const rect = surface.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;

    for (const overlay of overlays) {
      const overlayImage = await loadImage(overlay.src);
      context.drawImage(
        overlayImage,
        overlay.x * scaleX,
        overlay.y * scaleY,
        overlay.width * scaleX,
        overlay.height * scaleY
      );
    }

    return {
      previewUrl: canvas.toDataURL("image/png"),
      payload: {
        capturedAt: new Date().toISOString(),
        baseImage: {
          type: sourceType,
          label: sourceType === "camera" ? "live-camera-frame" : uploadedName ?? "uploaded-image"
        },
        stageSize: {
          width: canvas.width,
          height: canvas.height
        },
        overlays: overlays.map((overlay) => ({
          name: overlay.name,
          src: overlay.src,
          x: Number((overlay.x * scaleX).toFixed(2)),
          y: Number((overlay.y * scaleY).toFixed(2)),
          width: Number((overlay.width * scaleX).toFixed(2)),
          height: Number((overlay.height * scaleY).toFixed(2)),
          zIndex: overlay.zIndex
        }))
      }
    };
  }

  function render() {
    emptyState.hidden = sourceType !== "empty";
    video.hidden = sourceType !== "camera";
    image.hidden = sourceType !== "upload";
    startButton.disabled = sourceType === "camera";
    stopButton.disabled = sourceType !== "camera";
  }

  render();

  return {
    startCamera,
    stopCamera,
    loadUpload,
    exportPreview,
    hasBaseSource
  };
}

function readFileAsDataUrl(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(String(reader.result));
    reader.onerror = () => reject(new Error("Could not read the selected image."));
    reader.readAsDataURL(file);
  });
}

function loadImage(src) {
  return new Promise((resolve, reject) => {
    const image = new Image();
    image.onload = () => resolve(image);
    image.onerror = () => reject(new Error(`Could not load overlay asset: ${src}`));
    image.src = src;
  });
}
