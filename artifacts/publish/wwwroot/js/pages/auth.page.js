import { $$ } from "../core/dom.js";

export function initAuthPage() {
  $$("[data-password-toggle]").forEach((button) => {
    button.addEventListener("click", () => {
      const target = document.getElementById(button.dataset.target);
      if (!target) {
        return;
      }

      const shouldShow = target.type === "password";
      target.type = shouldShow ? "text" : "password";
      button.textContent = shouldShow ? "Hide" : "Show";
    });
  });

  const firstError = document.querySelector(".c-field__error:not(:empty)");
  if (firstError) {
    const input = firstError.closest(".c-field")?.querySelector("input, textarea");
    input?.focus();
  }
}
