# Improved Prompt for Building a Blazor Hybrid Virtual Chapter Loader with Custom Visibility Sensor

- **Build a Blazor Hybrid page** that displays a book divided into virtual chapters streamed asynchronously one-by-one.

- **Implement a minimal custom Visibility Sensor** using JavaScript’s `IntersectionObserver`:
  - JS exposes `observe(element, dotNetHelper)` to watch element visibility and notify Blazor via `dotNetHelper.invokeMethodAsync`.
  - JS exposes `unobserve(element)` to disconnect observers and free resources.
  - Keep JS minimal, focused only on visibility detection.

- **Create a reusable Blazor component (`VisibilitySensor`)**:
  - Takes an `ElementReference` internally.
  - On first render, calls JS `observe` passing a .NET object reference.
  - Implements `[JSInvokable]` method `NotifyVisibilityChanged(bool isVisible)` which triggers a callback to the parent component.
  - Cleans up observer on disposal.
  
- **Use the Visibility Sensor to track each chapter’s visibility**:
  - Chapters have an `IsVisible` flag updated on visibility changes.
  - Render each chapter inside a `VisibilitySensor` component.

- **Lazy-load chapters asynchronously**:
  - Maintain a list of loaded chapters.
  - Whenever **all loaded chapters are visible**, trigger loading the next chapter.
  - Append new chapters to the list and observe their visibility.

- **Add memory management**:
  - Once the user has scrolled **10 chapters beyond the earliest visible chapter**, remove those old chapters from memory and UI to keep resource usage low.

- **Bonus feature: Pronunciation toggle**:
  - Add a checkbox to toggle showing pronunciation as ruby text on all streamed verses.
  - When enabled, add ruby annotations (furigana-style) to the existing content dynamically.

- **Highlight the interaction flow and benefits**:
  - JS handles visibility detection with minimal code.
  - Blazor handles state and UI updates in an event-driven way.
  - No manual scroll calculations or debounced scroll events.
  - Efficient incremental loading and unloading of chapters.
  - Enhanced user experience with pronunciation toggling.

---

# Sample Implementation

The sample below demonstrates:

- Custom JS visibility observer (assumed loaded as `wwwroot/js/visibilityObserver.js`).
- Reusable `VisibilitySensor` Blazor component.
- A parent `BookScroller` component that streams chapters one at a time.
- Tracks visibility per chapter and loads more when all visible.
- Removes chapters more than 10 chapters behind the earliest visible.
- Checkbox to toggle ruby pronunciation on all content.

---

## 1. Add `wwwroot/js/visibilityObserver.js`

```js
window.visibilityObserver = {
  observe: (element, dotNetHelper) => {
    const observer = new IntersectionObserver(entries => {
      entries.forEach(entry => {
        dotNetHelper.invokeMethodAsync('NotifyVisibilityChanged', entry.isIntersecting);
      });
    }, { threshold: [0] });
    observer.observe(element);
    element._observer = observer;
  },
  unobserve: (element) => {
    if (element._observer) {
      element._observer.disconnect();
      delete element._observer;
    }
  }
};
```

## 2. Add visibilityObserver in your index.html or _Host.cshtml

```html
<script src="js/visibilityObserver.js"></script>
```