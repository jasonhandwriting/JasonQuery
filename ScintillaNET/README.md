# ScintillaNET

This directory is a vendored and locally maintained copy of **ScintillaNET**,
used by JasonQuery as its Windows Forms SQL editor control. It is compiled
from source as part of the JasonQuery solution.

## Origin

- Original author: **Jacob Slusser**
- Upstream repository: <https://github.com/jacobslusser/ScintillaNET>
- Upstream status: archived and read-only since 2023-12-22
- Managed wrapper license: MIT License

The source in this directory includes JasonQuery-specific integration and
maintenance changes. Jacob Slusser retains copyright in the original
ScintillaNET contributions.

## ScintillaNET Managed Wrapper License

Copyright (c) 2017, Jacob Slusser, <https://github.com/jacobslusser>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Native SciLexer Runtime

ScintillaNET uses the native **SciLexer.dll** runtime from the Scintilla
project. The runtime is included as compressed embedded resources and as
native files under `x64` and `x86`.

SciLexer is not covered by the MIT license above. Its separate Scintilla and
SciTE license is included unchanged in `x64/License.txt` and
`x86/License.txt`; these files must remain with every source or binary
distribution that includes the corresponding SciLexer runtime.

JasonQuery officially targets and ships **x64 only**. The `x86` directory is
retained in this vendored source snapshot for upstream completeness and is not
part of JasonQuery release packaging.

## Distribution Rules

- Keep this README and the applicable `x64/License.txt` or `x86/License.txt`
  files when redistributing this directory or its native runtime.
- Preserve the MIT copyright and permission notice for the managed wrapper.
- Preserve the separate Scintilla and SciTE notice for every included
  `SciLexer.dll` or `SciLexer.dll.gz` resource.

