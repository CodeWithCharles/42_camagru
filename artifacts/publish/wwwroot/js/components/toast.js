import { $, createElement } from "../core/dom.js";

export function initToasts() {
  const stack = $("[data-toast-stack]");
  if (!stack) {
    return;
  }

  stack.addEventListener("click", (event) => {
    const button = event.target.closest("[data-toast-dismiss]");
    if (!button) {
      return;
    }

    button.closest("[data-toast]")?.remove();
  });
}

export function pushToast({ kind = "info", title = "Camagru", message }) {
  let stack = $("[data-toast-stack]");
  if (!stack) {
    stack = createElement("section", "c-toast-stack");
    stack.setAttribute("data-toast-stack", "");
    stack.setAttribute("aria-live", "polite");
    document.body.appendChild(stack);
  }

  const toast = createElement("article", `c-toast c-toast--${kind}`);
  toast.setAttribute("data-toast", "");

  const content = createElement("div");
  const titleNode = createElement("strong", "", title);
  const messageNode = createElement("p", "", message);
  content.append(titleNode, messageNode);

  const dismiss = createElement("button", "c-toast__dismiss", "×");
  dismiss.type = "button";
  dismiss.setAttribute("data-toast-dismiss", "");
  dismiss.setAttribute("aria-label", "Dismiss notification");
  dismiss.addEventListener("click", () => {
    toast.remove();
  });

  toast.append(content, dismiss);
  stack.appendChild(toast);
}
