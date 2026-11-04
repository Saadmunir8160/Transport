---
activation: always_on
---
# Super Admin design rules (Smart Transport, Kashmir)
- NEVER hard-code colors, spacing, radii or font sizes outside the tokens file.
- NEVER use emoji as icons or icon-only navigation without an accessible label (aria-label + title).
- NEVER use Indian place names or data in mocks; use only Muzaffarabad, Mirpur, Kotli, Rawalakot, Bagh, Bhimber.
- NEVER change backend, database, auth logic or API contracts.
- NEVER change Transport Admin, Driver or Passenger layouts.
- NEVER install new UI libraries or icon sets without asking first.
- NEVER use ALL CAPS labels; sentence case only.
- Motion only in response to user actions; respect prefers-reduced-motion.
- Use CSS logical properties (margin-inline-start, padding-inline, etc.) so RTL/Urdu can be added later.
- Sticky top bar must never hide focused elements (use scroll-padding-top).
