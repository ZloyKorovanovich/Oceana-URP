# Oceana-URP
Oceana water for Unity URP

![image_2024-08-03_01-20-15](https://github.com/user-attachments/assets/ced76b34-9957-481c-8950-d2317d655119)

This is solution for Unity Universal Render Pipline. Supported version 2023.2+
Repository contains of source project files and unity package for simpler installation.

Features: water surface rendering with custom lighting model, underwater post-processing

YouTube preview: https://www.youtube.com/watch?v=dJbRxulwyZU&t=2s

Artstation preview: https://www.artstation.com/artwork/XJeZ3n

Solution contains:
- 2 Graphics shaders for ocean surface & underwater post-processing
- 2 Compute shaders for generating mesh & rendering ocean scrolls
- 5 C# scripts for handling CPU part of code
- 1 Demo Scene

Solution requiers:
- Unity ver. 2023.2+
- Universal RP
- FullScreen Render Feature (for underwater effects)

IMPORTANT NOTES: 
- For rendering ocean surface correctly toggles "Depth Texture" & "Opaque Texture" in URP Asset must be enabled.
- For rendering post processing toggle "Deferred render path" in URP renderer must be enabled.

If there will be any issues or bugs found in this asset, report them to this e-mail: mitrofan3452@gmail.com
