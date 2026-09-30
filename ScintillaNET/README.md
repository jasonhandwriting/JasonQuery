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
project. JasonQuery's vendored source includes the x64 runtime as
`x64/SciLexer.dll.gz`, together with `x64/License.txt` and `x64/version.txt`.

SciLexer is not covered by the MIT license above. Its separate Scintilla and
SciTE license is included unchanged in `x64/License.txt` and must remain with
source or binary distributions that include the corresponding SciLexer
runtime.

JasonQuery officially targets and ships **x64 only**. No x86 SciLexer runtime
is included in the current JasonQuery repository or release packaging.

## Distribution Rules

- Keep this README and `x64/License.txt` when redistributing this directory or
  its native runtime.
- Preserve the MIT copyright and permission notice for the managed wrapper.
- Preserve the separate Scintilla and SciTE notice for every included
  `SciLexer.dll` or `SciLexer.dll.gz` resource.
