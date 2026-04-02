import { $$ } from "../core/dom.js";

export function initAuthPage() {
  $$("[data-password-toggle]").forEach((button) => {
    button.addEventListener("click", () => {
      const target = resolvePasswordTarget(button);
      if (!target) {
        return;
      }

      const shouldShow = target.type === "password";
      target.type = shouldShow ? "text" : "password";
      button.textContent = shouldShow ? "Hide" : "Show";
      button.setAttribute("aria-pressed", shouldShow ? "true" : "false");
    });
  });

  const firstError = document.querySelector(".c-field__error:not(:empty)");
  if (firstError) {
    const input = firstError.closest(".c-field")?.querySelector("input, textarea");
    input?.focus();
  }
}

function resolvePasswordTarget(button) {
  if (button.dataset.target) {
    const targetById = document.getElementById(button.dataset.target);
    if (targetById instanceof HTMLInputElement) {
      return targetById;
    }
  }

  const localTarget = button.closest(".c-password-input")?.querySelector("input");
  return localTarget instanceof HTMLInputElement ? localTarget : null;
}
