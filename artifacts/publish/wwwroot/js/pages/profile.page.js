export function initProfilePage() {
  const activeSection = document.body.dataset.activeSection;
  if (!activeSection) {
    return;
  }

  const section = document.querySelector(`[data-profile-section="${activeSection}"]`);
  section?.scrollIntoView({ block: "start" });
}
