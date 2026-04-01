import { $$ } from "../core/dom.js";

export function initCarousel(root) {
  const slides = $$("[data-carousel-slide]", root);
  const dots = $$("[data-carousel-dot]", root);
  if (!slides.length) {
    return;
  }

  let index = slides.findIndex((slide) => slide.classList.contains("is-active"));
  index = index >= 0 ? index : 0;

  const render = () => {
    slides.forEach((slide, slideIndex) => {
      slide.classList.toggle("is-active", slideIndex === index);
    });

    dots.forEach((dot, dotIndex) => {
      dot.classList.toggle("is-active", dotIndex === index);
    });
  };

  root.addEventListener("click", (event) => {
    if (event.target.closest("[data-carousel-prev]")) {
      index = (index - 1 + slides.length) % slides.length;
      render();
      return;
    }

    if (event.target.closest("[data-carousel-next]")) {
      index = (index + 1) % slides.length;
      render();
      return;
    }

    const dot = event.target.closest("[data-carousel-dot]");
    if (dot) {
      index = Number(dot.dataset.carouselIndex);
      render();
    }
  });

  render();
}
