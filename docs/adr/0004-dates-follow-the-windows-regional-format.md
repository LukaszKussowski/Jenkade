# Dates and times follow the Windows regional format, not the UI language

Jenkade's UI language (English or Polish, from the Windows UI language or the settings override) decides only the UI text, including duration units such as "min" or "godz.". Dates, times and numbers use the Windows regional format (`CultureInfo.CurrentCulture`, with the user's own overrides such as a 24-hour clock or ISO dates). This is how Windows itself and Microsoft's globalization guidance separate the two settings: a user who set a regional format chose it deliberately.

## Considered options

- **Format with the UI language** (`en` or `pl`), the original wording of the MVP spec: everyone outside Poland gets US dates and a 12-hour clock (a British user sees `10/22/2026 2:05 PM`), and customised Windows formats are ignored.
- **Regional format only when its language matches the UI language**, otherwise the UI language's default format: fixes the British case but still ignores customised formats, and is hard to explain.

## Consequences

- Polish UI text can sit next to non-Polish dates (Polish UI on a US regional format shows `10/22/2026 2:05 PM`), as in Windows itself.
- The language override in settings changes UI text only; dates keep the regional format.
- `UiLanguage.Apply` sets only the UI culture; `DisplayFormat` formats with the current culture.
