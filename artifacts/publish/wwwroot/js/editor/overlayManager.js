export function createOverlayManager(initialState = []) {
  let overlays = initialState.map((overlay, index) => ({
    ...overlay,
    zIndex: overlay.zIndex ?? index + 1
  }));
  let selectedId = overlays.at(-1)?.id ?? null;
  const listeners = new Set();

  const notify = () => {
    const snapshot = {
      overlays: overlays
        .slice()
        .sort((left, right) => left.zIndex - right.zIndex)
        .map((overlay) => ({ ...overlay })),
      selectedId
    };

    listeners.forEach((listener) => listener(snapshot));
  };

  const setOverlays = (nextOverlays) => {
    overlays = normalize(nextOverlays);
    if (selectedId && !overlays.some((overlay) => overlay.id === selectedId)) {
      selectedId = overlays.at(-1)?.id ?? null;
    }
    notify();
  };

  return {
    addOverlay(input) {
      const offset = overlays.length * 18;
      const nextOverlay = {
        id: `overlay-${crypto.randomUUID()}`,
        name: input.name,
        src: input.src,
        accent: input.accent,
        x: 80 + offset,
        y: 80 + offset,
        width: 120,
        height: 120,
        zIndex: overlays.length + 1
      };

      overlays = normalize([...overlays, nextOverlay]);
      selectedId = nextOverlay.id;
      notify();
    },
    updateOverlay(id, patch) {
      setOverlays(overlays.map((overlay) => (overlay.id === id ? { ...overlay, ...patch } : overlay)));
    },
    removeOverlay(id) {
      setOverlays(overlays.filter((overlay) => overlay.id !== id));
    },
    moveLayer(id, direction) {
      const sorted = overlays.slice().sort((left, right) => left.zIndex - right.zIndex);
      const currentIndex = sorted.findIndex((overlay) => overlay.id === id);
      if (currentIndex < 0) {
        return;
      }

      const nextIndex = direction === "up" ? currentIndex + 1 : currentIndex - 1;
      if (nextIndex < 0 || nextIndex >= sorted.length) {
        return;
      }

      [sorted[currentIndex], sorted[nextIndex]] = [sorted[nextIndex], sorted[currentIndex]];
      setOverlays(sorted);
      selectedId = id;
    },
    select(id) {
      selectedId = id;
      notify();
    },
    subscribe(listener) {
      listeners.add(listener);
      listener({
        overlays: overlays.slice().sort((left, right) => left.zIndex - right.zIndex).map((overlay) => ({ ...overlay })),
        selectedId
      });
      return () => listeners.delete(listener);
    },
    getState() {
      return {
        overlays: overlays.slice().sort((left, right) => left.zIndex - right.zIndex).map((overlay) => ({ ...overlay })),
        selectedId
      };
    },
    hasOverlays() {
      return overlays.length > 0;
    }
  };
}

function normalize(overlays) {
  return overlays
    .slice()
    .sort((left, right) => left.zIndex - right.zIndex)
    .map((overlay, index) => ({
      ...overlay,
      zIndex: index + 1
    }));
}
