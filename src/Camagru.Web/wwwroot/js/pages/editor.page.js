import { $, $$, createElement } from "../core/dom.js";
import { loadJson, saveJson } from "../core/storage.js";
import { pushToast } from "../components/toast.js";
import { createOverlayManager } from "../editor/overlayManager.js";
import { attachDragResize } from "../editor/dragResize.js";
import { initLayersPanel } from "../editor/layersPanel.js";
import { createCaptureController } from "../editor/capture.js";

export function initEditorPage() {
  const root = $("[data-editor-root]");
  if (!root) {
    return;
  }

  const storageKey = root.dataset.editorStorage;
  const overlaySurface = $("[data-overlay-surface]", root);
  const stage = $("[data-editor-stage]", root);
  const video = $("[data-stage-video]", root);
  const image = $("[data-stage-image]", root);
  const emptyState = $("[data-stage-empty]", root);
  const uploadInput = $("[data-editor-upload]", root);
  const exportButton = $("[data-export-capture]", root);
  const startButton = $("[data-start-camera]", root);
  const stopButton = $("[data-stop-camera]", root);
  const previewImage = $("[data-export-preview]", root);
  const previewEmpty = $("[data-preview-empty]", root);
  const payloadOutput = $("[data-export-payload]", root);
  const thumbList = $("[data-thumb-list]", root);
  const thumbEmpty = $("[data-thumb-empty-state]", root);
  const layerPanel = $("[data-layer-panel]", root);

  const drafts = loadJson(storageKey, []);
  const manager = createOverlayManager();
  const captureController = createCaptureController({
    stage,
    video,
    image,
    emptyState,
    startButton,
    stopButton
  });

  const updateCaptureState = () => {
    exportButton.disabled = !(captureController.hasBaseSource() && manager.hasOverlays());
  };

  const renderSurface = ({ overlays, selectedId }) => {
    const knownIds = new Set(overlays.map((overlay) => overlay.id));
    $$("[data-overlay-id]", overlaySurface).forEach((node) => {
      if (!knownIds.has(node.dataset.overlayId)) {
        node.remove();
      }
    });

    overlays.forEach((overlay) => {
      let node = overlaySurface.querySelector(`[data-overlay-id="${overlay.id}"]`);
      if (!node) {
        node = createElement("div", "c-editor-overlay");
        node.dataset.overlayId = overlay.id;
        const imageNode = createElement("img");
        const label = createElement("span", "c-editor-overlay__label");
        label.textContent = overlay.name;

        node.append(imageNode, label);
        ["nw", "ne", "sw", "se"].forEach((handle) => {
          const handleNode = createElement("button", "c-editor-overlay__handle");
          handleNode.type = "button";
          handleNode.dataset.overlayHandle = handle;
          node.append(handleNode);
        });

        overlaySurface.appendChild(node);
      }

      node.classList.toggle("is-selected", overlay.id === selectedId);
      node.style.left = `${overlay.x}px`;
      node.style.top = `${overlay.y}px`;
      node.style.width = `${overlay.width}px`;
      node.style.height = `${overlay.height}px`;
      node.style.zIndex = String(overlay.zIndex);
      node.style.borderColor = overlay.accent;
      node.querySelector("img").src = overlay.src;
      node.querySelector(".c-editor-overlay__label").textContent = overlay.name;
    });

    updateCaptureState();
  };

  manager.subscribe(renderSurface);
  initLayersPanel({ root: layerPanel, manager });
  attachDragResize({ surface: overlaySurface, manager });

  startButton.addEventListener("click", async () => {
    try {
      await captureController.startCamera();
      updateCaptureState();
    } catch (error) {
      pushToast({
        kind: "info",
        title: "Webcam unavailable",
        message: error.message
      });
    }
  });

  stopButton.addEventListener("click", () => {
    captureController.stopCamera();
    updateCaptureState();
  });

  uploadInput.addEventListener("change", async () => {
    const file = uploadInput.files?.[0];
    if (!file) {
      return;
    }

    try {
      await captureController.loadUpload(file);
      updateCaptureState();
    } catch (error) {
      pushToast({
        kind: "error",
        title: "Upload failed",
        message: error.message
      });
    }
  });

  $$("[data-sticker-button]", root).forEach((button) => {
    button.addEventListener("click", () => {
      manager.addOverlay({
        name: button.dataset.stickerName,
        src: button.dataset.stickerSrc,
        accent: button.dataset.stickerAccent
      });
    });
  });

  exportButton.addEventListener("click", async () => {
    try {
      const snapshot = manager.getState();
      const exportResult = await captureController.exportPreview({
        overlays: snapshot.overlays,
        surface: overlaySurface
      });

      previewImage.hidden = false;
      previewImage.src = exportResult.previewUrl;
      previewEmpty.hidden = true;
      payloadOutput.textContent = JSON.stringify(exportResult.payload, null, 2);

      const draft = {
        id: `draft-${crypto.randomUUID()}`,
        title: `Capture ${drafts.length + 1}`,
        previewUrl: exportResult.previewUrl,
        capturedAtLabel: new Date().toLocaleString(),
        payload: exportResult.payload
      };

      drafts.unshift(draft);
      saveJson(storageKey, drafts.slice(0, 10));
      renderDrafts();

      pushToast({
        kind: "success",
        title: "Preview captured",
        message: "A local preview and payload snapshot were added to the capture tray."
      });
    } catch (error) {
      pushToast({
        kind: "error",
        title: "Capture blocked",
        message: error.message
      });
    }
  });

  thumbList.addEventListener("click", (event) => {
    const button = event.target.closest("[data-thumb-action]");
    if (!button) {
      return;
    }

    const draft = drafts.find((item) => item.id === button.dataset.thumbId);
    if (!draft) {
      return;
    }

    if (button.dataset.thumbAction === "view") {
      previewImage.hidden = false;
      previewImage.src = draft.previewUrl;
      previewEmpty.hidden = true;
      payloadOutput.textContent = JSON.stringify(draft.payload, null, 2);
      return;
    }

    if (button.dataset.thumbAction === "delete") {
      const nextDrafts = drafts.filter((item) => item.id !== draft.id);
      drafts.splice(0, drafts.length, ...nextDrafts);
      saveJson(storageKey, drafts);
      renderDrafts();
      pushToast({
        kind: "info",
        title: "Local draft removed",
        message: "Only drafts from this signed-in browser session can be deleted here."
      });
      return;
    }

    pushToast({
      kind: "info",
      title: "Publish staged",
      message: "Final publish is intentionally a placeholder until a server-side composition flow exists."
    });
  });

  function renderDrafts() {
    thumbList.innerHTML = "";
    thumbEmpty.hidden = drafts.length > 0;

    drafts.forEach((draft) => {
      const article = createElement("article", "c-card c-card--thumb");
      article.dataset.thumbItem = "";

      const imageNode = createElement("img");
      imageNode.src = draft.previewUrl;
      imageNode.alt = draft.title;

      const content = createElement("div");
      content.append(
        createElement("strong", "", draft.title),
        createElement("span", "", draft.capturedAtLabel)
      );

      const actions = createElement("div", "c-card--thumb__actions");
      actions.append(
        thumbButton("View", "view", draft.id),
        thumbButton("Delete", "delete", draft.id),
        thumbButton("Publish", "publish", draft.id)
      );

      article.append(imageNode, content, actions);
      thumbList.appendChild(article);
    });
  }

  function thumbButton(label, action, draftId) {
    const button = createElement("button", "c-button c-button--ghost", label);
    button.type = "button";
    button.dataset.thumbAction = action;
    button.dataset.thumbId = draftId;
    return button;
  }

  renderDrafts();
  updateCaptureState();
}
