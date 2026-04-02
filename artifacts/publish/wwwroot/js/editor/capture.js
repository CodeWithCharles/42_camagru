export function createCaptureController({ stage, video, image, emptyState, startButton, stopButton }) {
  let stream = null;
  let sourceType = "empty";
  let uploadedName = null;

  async function startCamera() {
    if (!navigator.mediaDevices?.getUserMedia) {
      throw new Error("Webcam preview is unavailable here. Upload a photo instead.");
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

  function buildPublishPayload({ overlays, surface }) {
    if (!hasBaseSource()) {
      throw new Error("Select a webcam or uploaded image first.");
    }

    const dimensions = getBaseDimensions();
    if (!dimensions.width || !dimensions.height) {
      throw new Error("The base image dimensions are not ready yet.");
    }

    const rect = surface.getBoundingClientRect();
    const scaleX = dimensions.width / rect.width;
    const scaleY = dimensions.height / rect.height;

    return {
      capturedAt: new Date().toISOString(),
      baseImage: {
        type: sourceType,
        label: sourceType === "camera" ? "live-camera-frame" : uploadedName ?? "uploaded-image"
      },
      stageSize: {
        width: dimensions.width,
        height: dimensions.height
      },
      overlays: overlays
        .slice()
        .sort((left, right) => left.zIndex - right.zIndex)
        .map((overlay) => ({
          overlayId: Number(overlay.overlayId ?? 0),
          name: overlay.name,
          src: overlay.src,
          x: Number((overlay.x * scaleX).toFixed(2)),
          y: Number((overlay.y * scaleY).toFixed(2)),
          width: Number((overlay.width * scaleX).toFixed(2)),
          height: Number((overlay.height * scaleY).toFixed(2)),
          zIndex: overlay.zIndex,
          rotationDegrees: Number((overlay.rotationDegrees ?? 0).toFixed(2))
        }))
    };
  }

  async function exportPreview({ overlays, surface }) {
    const payload = buildPublishPayload({ overlays, surface });
    const baseCanvas = await createBaseCanvas();
    const previewCanvas = document.createElement("canvas");
    previewCanvas.width = baseCanvas.width;
    previewCanvas.height = baseCanvas.height;

    const previewContext = previewCanvas.getContext("2d");
    if (!previewContext) {
      throw new Error("Canvas export is unavailable.");
    }

    previewContext.drawImage(baseCanvas, 0, 0);

    for (const overlay of payload.overlays) {
      const overlayImage = await loadImage(overlay.src);
      previewContext.drawImage(
        overlayImage,
        overlay.x,
        overlay.y,
        overlay.width,
        overlay.height
      );
    }

    return {
      previewUrl: previewCanvas.toDataURL("image/png"),
      baseImageDataUrl: baseCanvas.toDataURL("image/png"),
      payload
    };
  }

  async function createBaseImageFile() {
    const baseCanvas = await createBaseCanvas();
    const blob = await canvasToBlob(baseCanvas);
    return new File([blob], resolveBaseFileName(), { type: "image/png" });
  }

  function render() {
    emptyState.hidden = sourceType !== "empty";
    video.hidden = sourceType !== "camera";
    image.hidden = sourceType !== "upload";
    startButton.disabled = sourceType === "camera";
    stopButton.disabled = sourceType !== "camera";
  }

  async function createBaseCanvas() {
    if (!hasBaseSource()) {
      throw new Error("Select a webcam or uploaded image first.");
    }

    const { width, height } = getBaseDimensions();
    const canvas = document.createElement("canvas");
    canvas.width = width;
    canvas.height = height;

    const context = canvas.getContext("2d");
    if (!context) {
      throw new Error("Canvas export is unavailable.");
    }

    if (sourceType === "camera") {
      context.drawImage(video, 0, 0, canvas.width, canvas.height);
    } else {
      context.drawImage(image, 0, 0, canvas.width, canvas.height);
    }

    return canvas;
  }

  function getBaseDimensions() {
    if (sourceType === "camera") {
      return {
        width: video.videoWidth,
        height: video.videoHeight
      };
    }

    return {
      width: image.naturalWidth,
      height: image.naturalHeight
    };
  }

  function resolveBaseFileName() {
    if (sourceType === "camera") {
      return `camera-capture-${Date.now()}.png`;
    }

    if (!uploadedName) {
      return "uploaded-image.png";
    }

    return uploadedName.replace(/\.[a-z0-9]+$/i, "") + ".png";
  }

  render();

  return {
    startCamera,
    stopCamera,
    loadUpload,
    exportPreview,
    hasBaseSource,
    buildPublishPayload,
    createBaseImageFile
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

function canvasToBlob(canvas) {
  return new Promise((resolve, reject) => {
    canvas.toBlob((blob) => {
      if (!blob) {
        reject(new Error("Could not capture the base image."));
        return;
      }

      resolve(blob);
    }, "image/png");
  });
}
