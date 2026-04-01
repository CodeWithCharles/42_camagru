import { createElement } from "../core/dom.js";

export function initLayersPanel({ root, manager }) {
  const list = root.querySelector("[data-layer-list]");
  const emptyState = root.querySelector("[data-layer-empty-state]");

  const render = ({ overlays, selectedId }) => {
    list.innerHTML = "";
    emptyState.hidden = overlays.length > 0;

    overlays
      .slice()
      .reverse()
      .forEach((overlay) => {
        const item = createElement("li", "c-layer-panel__item");
        item.classList.toggle("is-selected", overlay.id === selectedId);

        const title = createElement("strong", "", overlay.name);
        const subtitle = createElement("span", "", `z${overlay.zIndex} · ${Math.round(overlay.width)}×${Math.round(overlay.height)}`);
        const controls = createElement("div", "c-layer-panel__controls");

        controls.append(
          button("Select", "select", overlay.id),
          button("Up", "up", overlay.id),
          button("Down", "down", overlay.id),
          button("Remove", "remove", overlay.id)
        );

        item.append(title, subtitle, controls);
        list.appendChild(item);
      });
  };

  root.addEventListener("click", (event) => {
    const button = event.target.closest("[data-layer-action]");
    if (!button) {
      return;
    }

    const { layerAction, layerId } = button.dataset;
    if (layerAction === "select") {
      manager.select(layerId);
    } else if (layerAction === "up") {
      manager.moveLayer(layerId, "up");
    } else if (layerAction === "down") {
      manager.moveLayer(layerId, "down");
    } else if (layerAction === "remove") {
      manager.removeOverlay(layerId);
    }
  });

  return manager.subscribe(render);
}

function button(label, action, id) {
  const element = createElement("button", "c-button c-button--ghost", label);
  element.type = "button";
  element.dataset.layerAction = action;
  element.dataset.layerId = id;
  return element;
}
