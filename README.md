# BLOXCity Renderer
A gift from BLOXCity to the entire sandbox community

Something that many websites need but don't have due to complex requirements

The avatar renderer... one of the most cruical parts of a sandbox game

## Features

- Handles Caching
- Can fetch resources over the Internet and put them into a local cache, you can also fetch local files
- Supports Vertex Colors
- Internal backlog: So if renders are spammed your server won't crash
- Very fast for what it does: 30 frames per second on my i5 1235u laptop with software rendering using llvmpipe
- Supports both Linux and Windows

## Notes

Lighting isn't very good, you should bake GI to vertex colors using Blender otherwise it'll look like something from 2008
but you can avoid doing that if you are for the retro looks

## Setup

This project should be OS and CPU Architecture agnostic meaning it should run anywhere .NET Core is supported although some platforms may require compiling native dependencies like GLFW manually

.NET Core works on Linux, Windows, FreeBSD and for cpu architectures ordered by support status: x86_64, x86, arm64, arm, ppc64le, s390x, loongarch

### Linux
A docker container is provded for convenience, if you don't want to use a container you can deduct steps from Dockerfile or follow the rough guide:

Getting up and ready should be easy as `dotnet publish -c Release` and copying the publish folder contents in a server

then install Xvfb

hit the executable with environment variables

`LIBGL_ALWAYS_SOFTWARE=1 GALLIUM_DRIVER=llvmpipe dotnet ./BLOXCityRenderer.dll`

If you didn't work with dotnet before .NET DLLs aren't classic Windows DLLs they run anywhere

### Windows
You need to use [mesa-dist-win](https://github.com/pal1000/mesa-dist-win) or [mesa3d for windows](https://fdossena.com/?p=mesa/index.frag)

Then simply run the exe in publish folder by compiling this with Visual Studio or `dotnet publish -c Release -r win-x64` after copying dlls or running the renderer software by dragging and dropping into it

### FreeBSD

.NET Core isn't supported on FreeBSD officially there is a community version of dotnet but you should able to follow the Linux guide and adapt it to working, if you encounter issues hit me up on BLOXCity Discord

You probably need to compile GLFW

## Usage

After setting up navigate to

`http://localhost:1444/?Head:Color=%23ffffff&Torso:Color=%23ffffff&LArm:Color=%23ffffff&RArm:Color=%23ffffff&LLeg:Color=%23ffffff&RLeg:Color=%23ffffff&Shirt=file:///assets/textures/staging/shirt_template2.png`

for docker users localhost would be the container's internal IP Address

Parameters are self explanatory

The api returns a webp image and a lqip code [css-lqip](https://leanrada.com/notes/css-only-lqip/)

You can save webp image into a S3 bucket or save it to your local filesystem

## Info

Code is messy because this is a rough port of an earlier project

Port of ancient Avatar Renderer from 2020 written in C++ to C#

There are a lot of things you can add like API Keys, Replace Skia dependency with something that is lighter, optimize the rendering further, refactor the rendering logic

## About BLOXCity

BLOXCity is currently in development platform exploring the niche of Social Media + Gaming

Visit our Discord Server to learn more: [Discord](https://discord.gg/8apPrvm9mB)

## License

GNU Lesser General Public License except for art assets

What does that mean:
- You can freely integrate this in your own project
- You do not need to open source your modified version's source code as long as you keep them internal meaning you can keep this closed sourced if you don't give someone else other than your employees executable files like exe and dll files
- You don't pay for any licensing
- You cannot relicense the source code into something else if you release your changes as open source again, they must stay under LGPL
- You cannot use the art assets, these remain as property of BLOXCity provided only for demo purposes you should replace them with your own avatar model instead


I plan to rewrite this for fourth time in the future in Rust instead and release this as a library for rendering small visualizations in general