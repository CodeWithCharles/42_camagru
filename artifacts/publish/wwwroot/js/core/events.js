export function delegate(root, eventName, selector, handler) {
  const listener = (event) => {
    const target = event.target.closest(selector);
    if (!target || !root.contains(target)) {
      return;
    }

    handler(event, target);
  };

  root.addEventListener(eventName, listener);
  return () => root.removeEventListener(eventName, listener);
}

export function emit(root, eventName, detail = {}) {
  root.dispatchEvent(new CustomEvent(eventName, { detail }));
}
