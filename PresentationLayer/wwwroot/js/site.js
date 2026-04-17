document.addEventListener("DOMContentLoaded", () => {
  const cartForms = document.querySelectorAll(".js-auto-cart-form");

  cartForms.forEach((form) => {
    const quantityInput = form.querySelector(".js-cart-qty");
    if (!quantityInput) {
      return;
    }

    let submitTimer = null;
    let lastValue = String(quantityInput.value ?? "");

    const submitIfChanged = () => {
      const parsed = Number.parseInt(String(quantityInput.value ?? "1"), 10);
      const normalizedValue = Number.isFinite(parsed) && parsed > 0 ? parsed : 1;
      quantityInput.value = String(normalizedValue);

      if (lastValue === quantityInput.value) {
        return;
      }

      lastValue = quantityInput.value;
      form.requestSubmit();
    };

    quantityInput.addEventListener("input", () => {
      if (submitTimer) {
        clearTimeout(submitTimer);
      }

      submitTimer = setTimeout(submitIfChanged, 350);
    });

    quantityInput.addEventListener("change", submitIfChanged);
    quantityInput.addEventListener("blur", submitIfChanged);
  });

  const carousels = document.querySelectorAll("[data-carousel]");

  carousels.forEach((carousel) => {
    const mainImage = carousel.querySelector("[data-carousel-main]");
    const thumbnails = Array.from(carousel.querySelectorAll("[data-carousel-thumb]"));
    const prevButton = carousel.querySelector("[data-carousel-prev]");
    const nextButton = carousel.querySelector("[data-carousel-next]");

    if (!mainImage || thumbnails.length === 0) {
      return;
    }

    let currentIndex = 0;

    const setActiveImage = (index) => {
      currentIndex = (index + thumbnails.length) % thumbnails.length;
      const nextUrl = thumbnails[currentIndex].dataset.imageUrl;

      if (!nextUrl) {
        return;
      }

      mainImage.src = nextUrl;
      thumbnails.forEach((thumb, thumbIndex) => {
        thumb.classList.toggle("active", thumbIndex === currentIndex);
      });
    };

    thumbnails.forEach((thumb, index) => {
      thumb.addEventListener("click", () => setActiveImage(index));
    });

    prevButton?.addEventListener("click", () => setActiveImage(currentIndex - 1));
    nextButton?.addEventListener("click", () => setActiveImage(currentIndex + 1));
  });

  const fileInputs = document.querySelectorAll("[data-file-input]");

  fileInputs.forEach((wrapper) => {
    const nativeInput = wrapper.querySelector("[data-file-input-native]");
    const triggerButton = wrapper.querySelector("[data-file-input-trigger]");
    const statusLabel = wrapper.querySelector("[data-file-input-status]");

    if (!nativeInput || !triggerButton || !statusLabel) {
      return;
    }

    const updateStatus = () => {
      const filesCount = nativeInput.files?.length ?? 0;
      if (filesCount === 0) {
        statusLabel.textContent = "Няма избрани файлове";
        return;
      }

      if (filesCount === 1) {
        const fileName = nativeInput.files?.[0]?.name ?? "";
        statusLabel.textContent = `Избран файл: ${fileName}`;
        return;
      }

      statusLabel.textContent = `Избрани файлове: ${filesCount}`;
    };

    triggerButton.addEventListener("click", () => {
      nativeInput.click();
    });

    nativeInput.addEventListener("change", updateStatus);
    wrapper.closest("form")?.addEventListener("reset", updateStatus);

    updateStatus();
  });
});
