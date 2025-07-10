#!/bin/sh
cd renderer && LIBGL_ALWAYS_SOFTWARE=1 GALLIUM_DRIVER=llvmpipe xvfb-run dotnet ./BLOXCityRenderer.dll
