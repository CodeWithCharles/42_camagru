import { initToasts } from "./components/toast.js";
import { initAuthPage } from "./pages/auth.page.js";
import { initGalleryPage } from "./pages/gallery.page.js";
import { initEditorPage } from "./pages/editor.page.js";
import { initProfilePage } from "./pages/profile.page.js";

initToasts();

const page = document.body.dataset.page;
if (page === "auth") {
  initAuthPage();
} else if (page === "gallery") {
  initGalleryPage();
} else if (page === "editor") {
  initEditorPage();
} else if (page === "profile") {
  initProfilePage();
}
