import { trapFocus } from "../core/focusTrap.js";

export function initModal(root, { onClose } = {}) {
  if (!root) {
    return null;
  }

  const panel = root.querySelector(".c-modal__panel");
  const releaseFocus = panel ? trapFocus(panel) : () => {};

  const close = () => {
    releaseFocus();
    onClose?.();
  };

  const onKeyDown = (event) => {
    if (event.key === "Escape") {
      close();
    }
  };

  root.addEventListener("click", (event) => {
    if (event.target.closest("[data-modal-close]")) {
      close();
    }
  });

  document.addEventListener("keydown", onKeyDown);

  return () => {
    releaseFocus();
    document.removeEventListener("keydown", onKeyDown);
  };
}
