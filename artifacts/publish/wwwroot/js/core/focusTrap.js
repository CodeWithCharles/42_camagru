const FOCUSABLE = [
  "a[href]",
  "button:not([disabled])",
  "input:not([disabled])",
  "textarea:not([disabled])",
  "select:not([disabled])",
  "[tabindex]:not([tabindex='-1'])"
].join(", ");

export function trapFocus(container) {
  const previouslyFocused = document.activeElement;

  function onKeyDown(event) {
    if (event.key !== "Tab") {
      return;
    }

    const focusable = Array.from(container.querySelectorAll(FOCUSABLE));
    if (!focusable.length) {
      event.preventDefault();
      return;
    }

    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault();
      first.focus();
    }
  }

  container.addEventListener("keydown", onKeyDown);
  const initialFocus = container.querySelector(FOCUSABLE);
  initialFocus?.focus();

  return () => {
    container.removeEventListener("keydown", onKeyDown);
    previouslyFocused?.focus?.();
  };
}
