Filename: FileHeaderConvention.md
Last Updated: 2025-07-17 07:15 CEST
Version: 1.1.0
State: Stable
Signed: User

Synopsis:
Defines the official file header convention for the Audiobook Compressor project.

---

# File Header Convention

This document establishes the official file header convention for the Audiobook Compressor project. The purpose of this header is to provide immediate, at-a-glance information about the status of any given file. Adhering to this convention ensures consistency, clarity, and aids in version tracking and potential forensics across all file types.

---

### C# Files (`.cs`)

This section defines the header format for all C# source code files. The header is placed within a standard multi-line comment block (`/* ... */`) at the very top of the file so it is ignored by the compiler.

```csharp
/*
    Filename: MainWindow.xaml.cs
    Last Updated: 2025-07-21 02:54 CEST
    Version: 1.1.0
    State: Stable
    Signed: User

    Synopsis:
    Brief description of the file's purpose and last changes.
*/
```

### XAML Files (`.xaml`)

This section defines the header format for all XAML markup files. The header is placed within a standard XML/XAML comment block (`<!-- ... -->`) at the very top of the file so it is ignored by the parser.

```xml
<!--
    Filename: MainWindow.xaml
    Last Updated: 2025-07-21 02:54 CEST
    Version: 1.1.0
    State: Stable
    Signed: User

    Synopsis:
    Brief description of the file's purpose and last changes.
-->
```

### Markdown Files (`.md`)

This section defines the header format for all Markdown documentation files. As Markdown does not have a formal comment syntax, the header is a plain text block at the very top of the file. A horizontal rule (`---`) is used to create a clean visual separation between the header and the document's content.

```markdown
Filename: Summary.md
Last Updated: 2025-07-21 02:54 CEST
Version: 1.1.0
State: Stable
Signed: User

Synopsis:
Brief description of the file's purpose and last changes.

---
