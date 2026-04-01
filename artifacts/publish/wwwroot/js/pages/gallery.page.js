import { $$ } from "../core/dom.js";
import { initCarousel } from "../components/carousel.js";
import { initModal } from "../components/modal.js";
import { pushToast } from "../components/toast.js";

export function initGalleryPage() {
  $$("[data-carousel]").forEach(initCarousel);

  const modalRoot = document.querySelector("[data-modal-root]");
  if (modalRoot) {
    initModal(modalRoot, {
      onClose: () => {
        window.location.assign(modalRoot.dataset.closeUrl);
      }
    });
  }

  $$("[data-share-button]").forEach((button) => {
    button.addEventListener("click", async () => {
      const shareUrl = new URL(button.dataset.shareUrl, window.location.origin).toString();
      const shareTitle = button.dataset.shareTitle ?? "Camagru post";

      try {
        if (navigator.share) {
          await navigator.share({ title: shareTitle, url: shareUrl });
        } else if (navigator.clipboard?.writeText) {
          await navigator.clipboard.writeText(shareUrl);
          pushToast({
            kind: "success",
            title: "Link copied",
            message: "The gallery deep link is ready to share."
          });
        } else {
          pushToast({
            kind: "info",
            title: "Share unavailable",
            message: shareUrl
          });
        }
      } catch {
        pushToast({
          kind: "info",
          title: "Share dismissed",
          message: "The post stays available through its deep-linkable gallery URL."
        });
      }
    });
  });
}
