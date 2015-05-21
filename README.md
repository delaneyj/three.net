## three.net
#C# port of [Three.js](http://threejs.org/)

###Motivation
C# is a wonderful language.  It has the tools builtin in to allow for fast and robust development.  Straight up its an amazing language for writing games in.  While there are some game engine that utlize C# such and Unity, Paradox, Mono they are very oppiniated on your entire stack.  In the land of C++ you have a plethora of [do one thing well libraries](http://www.catb.org/esr/writings/taoup/html/ch01s06.html) and unfortunately for cross platform OpenGL graphics in C# this is a severly lacking space.

[Three.js](http://threejs.org/) is purpose built Javascript library for creating 3d graphics primarily for the HTML5 canvas.  It is small (~16K lines of code) and only supports modern OpenGL (vertex buffer, no fixed function pipeline, etc).  It seemed that if javascript has a better graphics engine than C# that was a project worth tackling. 

###Goals
* Provide a graphics only OpenGL 3d libraries for C# 5+
* Port all functionality from Three.js verbatim if possible
* Start add more elegant helpers/features after port is complete.

###Status
The core features are almost all there (buffers, geometry, lighting, normalmapping, etc).  The two major holes are bone/vertex animation, render buffers and deferred lighting.  These should not be anymore difficult than the previous bulk of code but just haven't been tackled yet.  Given that you can step through a browser dev window and see the exact call stack and parameters of the low level calls has made the port a relatively easy process. 

###Help wanted
I'd love the help of individuals that can read/write both in Javascript and C#.  Right now as close to a verbatim port is the goal but once we are feature complete a reworking to *sharpify* the codebase will help everyone.

After that I would like to focus on adding extra orthogonal features that together can be used to form a game engine.