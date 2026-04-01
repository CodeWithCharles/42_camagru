export function attachDragResize({ surface, manager }) {
  let interaction = null;

  const onPointerDown = (event) => {
    const overlay = event.target.closest("[data-overlay-id]");
    if (!overlay) {
      manager.select(null);
      return;
    }

    const overlayId = overlay.dataset.overlayId;
    const handle = event.target.closest("[data-overlay-handle]");
    const { overlays } = manager.getState();
    const current = overlays.find((item) => item.id === overlayId);
    if (!current) {
      return;
    }

    interaction = {
      id: overlayId,
      mode: handle ? "resize" : "drag",
      handle: handle?.dataset.overlayHandle ?? null,
      startX: event.clientX,
      startY: event.clientY,
      snapshot: current
    };

    manager.select(overlayId);
    overlay.setPointerCapture?.(event.pointerId);
    event.preventDefault();
  };

  const onPointerMove = (event) => {
    if (!interaction) {
      return;
    }

    const dx = event.clientX - interaction.startX;
    const dy = event.clientY - interaction.startY;
    const current = interaction.snapshot;

    if (interaction.mode === "drag") {
      manager.updateOverlay(interaction.id, {
        x: Math.max(0, current.x + dx),
        y: Math.max(0, current.y + dy)
      });
      return;
    }

    const resize = computeResize(current, interaction.handle, dx, dy);
    manager.updateOverlay(interaction.id, resize);
  };

  const onPointerEnd = () => {
    interaction = null;
  };

  surface.addEventListener("pointerdown", onPointerDown);
  surface.addEventListener("pointermove", onPointerMove);
  surface.addEventListener("pointerup", onPointerEnd);
  surface.addEventListener("pointercancel", onPointerEnd);

  return () => {
    surface.removeEventListener("pointerdown", onPointerDown);
    surface.removeEventListener("pointermove", onPointerMove);
    surface.removeEventListener("pointerup", onPointerEnd);
    surface.removeEventListener("pointercancel", onPointerEnd);
  };
}

function computeResize(overlay, handle, dx, dy) {
  const minSize = 56;
  const next = {
    x: overlay.x,
    y: overlay.y,
    width: overlay.width,
    height: overlay.height
  };

  if (handle?.includes("e")) {
    next.width = Math.max(minSize, overlay.width + dx);
  }

  if (handle?.includes("s")) {
    next.height = Math.max(minSize, overlay.height + dy);
  }

  if (handle?.includes("w")) {
    const width = Math.max(minSize, overlay.width - dx);
    next.x = overlay.x + (overlay.width - width);
    next.width = width;
  }

  if (handle?.includes("n")) {
    const height = Math.max(minSize, overlay.height - dy);
    next.y = overlay.y + (overlay.height - height);
    next.height = height;
  }

  return next;
}
